using System.Text.Json;
using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public class WhatsAppInboundHandler : IWhatsAppInboundHandler
{
    private const int MaxListRows = 10;
    private const string ProjectRowPrefix = "project:";
    private const string PageRowPrefix = "page:";

    private static readonly HashSet<string> MenuCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "hi", "hello", "hey", "start", "menu", "projects", "help", "cancel", "stop", "restart"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWhatsAppContactRepository _contactRepository;
    private readonly IWhatsAppConversationSessionRepository _sessionRepository;
    private readonly IProcessedWhatsAppMessageRepository _processedMessageRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IWhatsAppCloudApiClient _cloudApiClient;
    private readonly IAiTaskWorkflowService _workflowService;
    private readonly IUserRepository _userRepository;
    private readonly AISettings _aiSettings;
    private readonly WhatsAppSettings _whatsAppSettings;
    private readonly ILogger<WhatsAppInboundHandler> _logger;

    public WhatsAppInboundHandler(
        IWhatsAppContactRepository contactRepository,
        IWhatsAppConversationSessionRepository sessionRepository,
        IProcessedWhatsAppMessageRepository processedMessageRepository,
        IProjectRepository projectRepository,
        IWhatsAppCloudApiClient cloudApiClient,
        IAiTaskWorkflowService workflowService,
        IUserRepository userRepository,
        AISettings aiSettings,
        WhatsAppSettings whatsAppSettings,
        ILogger<WhatsAppInboundHandler> logger)
    {
        _contactRepository = contactRepository;
        _sessionRepository = sessionRepository;
        _processedMessageRepository = processedMessageRepository;
        _projectRepository = projectRepository;
        _cloudApiClient = cloudApiClient;
        _workflowService = workflowService;
        _userRepository = userRepository;
        _aiSettings = aiSettings;
        _whatsAppSettings = whatsAppSettings;
        _logger = logger;
    }

    public async Task HandleWebhookPayloadAsync(
        string rawJsonBody,
        CancellationToken cancellationToken = default)
    {
        WhatsAppWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WhatsAppWebhookPayload>(rawJsonBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize WhatsApp webhook payload.");
            return;
        }

        if (payload?.Entry is null || payload.Entry.Count == 0)
        {
            return;
        }

        foreach (var entry in payload.Entry)
        {
            if (entry.Changes is null)
            {
                continue;
            }

            foreach (var change in entry.Changes)
            {
                if (!string.Equals(change.Field, "messages", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = change.Value;
                if (value?.Messages is null || value.Messages.Count == 0)
                {
                    continue;
                }

                var phoneNumberId = value.Metadata?.PhoneNumberId;
                if (string.IsNullOrWhiteSpace(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp webhook missing metadata.phone_number_id.");
                    continue;
                }

                foreach (var message in value.Messages)
                {
                    await ProcessMessageAsync(phoneNumberId, message, cancellationToken);
                }
            }
        }
    }

    private async Task ProcessMessageAsync(
        string phoneNumberId,
        WhatsAppInboundMessage message,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Id) || string.IsNullOrWhiteSpace(message.From))
        {
            return;
        }

        var messageType = message.Type?.Trim().ToLowerInvariant();
        if (messageType is not ("text" or "image" or "interactive"))
        {
            _logger.LogDebug(
                "Ignoring WhatsApp message {MessageId} of type {Type}.",
                message.Id,
                message.Type);
            return;
        }

        if (await _processedMessageRepository.ExistsAsync(message.Id, cancellationToken))
        {
            _logger.LogInformation(
                "Skipping already processed WhatsApp message {MessageId}.",
                message.Id);
            return;
        }

        var contact = await _contactRepository.GetActiveByPhoneNumberIdAsync(
            phoneNumberId,
            cancellationToken);

        if (contact is null)
        {
            _logger.LogWarning(
                "No active WhatsAppContact for phone_number_id {PhoneNumberId}.",
                phoneNumberId);
            await TryReplyAsync(
                phoneNumberId,
                message.From,
                "This WhatsApp number is not configured. Ask an admin to enable it.",
                cancellationToken);
            await MarkProcessedAsync(message.Id, taskId: null, cancellationToken);
            return;
        }

        if (!_aiSettings.Enabled)
        {
            await TryReplyAsync(
                phoneNumberId,
                message.From,
                "AI task creation is currently disabled.",
                cancellationToken);
            await MarkProcessedAsync(message.Id, taskId: null, cancellationToken);
            return;
        }

        try
        {
            if (messageType == "interactive")
            {
                await HandleInteractiveAsync(phoneNumberId, message, cancellationToken);
            }
            else if (IsMenuCommand(message))
            {
                await _sessionRepository.DeleteAsync(phoneNumberId, message.From, cancellationToken);
                await SendProjectPickerAsync(phoneNumberId, message.From, page: 0, cancellationToken);
            }
            else
            {
                var session = await GetActiveSessionAsync(
                    phoneNumberId,
                    message.From,
                    cancellationToken);

                if (session is null)
                {
                    if (messageType == "text"
                        && await TrySelectProjectByNameAsync(
                            phoneNumberId,
                            message.From,
                            message.Text?.Body,
                            cancellationToken))
                    {
                        // Project selected by typed name.
                    }
                    else
                    {
                        await SendProjectPickerAsync(phoneNumberId, message.From, page: 0, cancellationToken);
                    }
                }
                else
                {
                    await CreateTaskFromFeedbackAsync(
                        phoneNumberId,
                        message,
                        session,
                        cancellationToken);
                    return;
                }
            }

            await MarkProcessedAsync(message.Id, taskId: null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process WhatsApp message {MessageId}.",
                message.Id);

            await TryReplyAsync(
                phoneNumberId,
                message.From,
                "Sorry, something went wrong. Reply menu to start again.",
                cancellationToken);
            await MarkProcessedAsync(message.Id, taskId: null, cancellationToken);
        }
    }

    private async Task HandleInteractiveAsync(
        string phoneNumberId,
        WhatsAppInboundMessage message,
        CancellationToken cancellationToken)
    {
        var replyId = message.Interactive?.ListReply?.Id
            ?? message.Interactive?.ButtonReply?.Id;

        if (string.IsNullOrWhiteSpace(replyId))
        {
            await SendProjectPickerAsync(phoneNumberId, message.From!, page: 0, cancellationToken);
            return;
        }

        if (replyId.StartsWith(PageRowPrefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(replyId[PageRowPrefix.Length..], out var page)
            && page >= 0)
        {
            await SendProjectPickerAsync(phoneNumberId, message.From!, page, cancellationToken);
            return;
        }

        if (replyId.StartsWith(ProjectRowPrefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(replyId[ProjectRowPrefix.Length..], out var projectId)
            && projectId > 0)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project is null)
            {
                await TryReplyAsync(
                    phoneNumberId,
                    message.From!,
                    "That project no longer exists. Please choose another.",
                    cancellationToken);
                await SendProjectPickerAsync(phoneNumberId, message.From!, page: 0, cancellationToken);
                return;
            }

            await BeginAwaitingFeedbackAsync(
                phoneNumberId,
                message.From!,
                project,
                cancellationToken);
            return;
        }

        await SendProjectPickerAsync(phoneNumberId, message.From!, page: 0, cancellationToken);
    }

    private async Task<bool> TrySelectProjectByNameAsync(
        string phoneNumberId,
        string senderWaId,
        string? typedName,
        CancellationToken cancellationToken)
    {
        var name = typedName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var projects = (await _projectRepository.GetAllAsync()).ToList();
        var matches = projects
            .Where(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count != 1)
        {
            return false;
        }

        await BeginAwaitingFeedbackAsync(
            phoneNumberId,
            senderWaId,
            matches[0],
            cancellationToken);
        return true;
    }

    private async Task BeginAwaitingFeedbackAsync(
        string phoneNumberId,
        string senderWaId,
        Project project,
        CancellationToken cancellationToken)
    {
        var session = new WhatsAppConversationSession(
            phoneNumberId,
            senderWaId,
            project.Id,
            project.Name);

        await _sessionRepository.UpsertAsync(session, cancellationToken);

        await TryReplyAsync(
            phoneNumberId,
            senderWaId,
            $"Project selected: *{project.Name}*\n\n"
            + "Send a short description of the task. "
            + "You can attach an image in the same message (photo with a caption).\n\n"
            + "Reply *menu* to pick a different project, or *cancel* to stop.",
            cancellationToken);
    }

    private async Task CreateTaskFromFeedbackAsync(
        string phoneNumberId,
        WhatsAppInboundMessage message,
        WhatsAppConversationSession session,
        CancellationToken cancellationToken)
    {
        var botUser = await _userRepository.GetByEmailAsync(WhatsAppSettings.BotEmail);
        if (botUser is null)
        {
            _logger.LogError(
                "WhatsApp bot user {Email} was not found. Cannot create task history.",
                WhatsAppSettings.BotEmail);
            await TryReplyAsync(
                phoneNumberId,
                message.From!,
                "WhatsApp bot is not configured. Contact an administrator.",
                cancellationToken);
            await MarkProcessedAsync(message.Id!, taskId: null, cancellationToken);
            return;
        }

        string feedbackText;
        ImageUploadRequest? image = null;
        MemoryStream? imageStream = null;
        var messageType = message.Type?.Trim().ToLowerInvariant();

        try
        {
            if (messageType == "text")
            {
                feedbackText = message.Text?.Body?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(feedbackText))
                {
                    await TryReplyAsync(
                        phoneNumberId,
                        message.From!,
                        "Please send a description (you can also attach an image).",
                        cancellationToken);
                    await MarkProcessedAsync(message.Id!, taskId: null, cancellationToken);
                    return;
                }
            }
            else
            {
                feedbackText = message.Image?.Caption?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(message.Image?.Id))
                {
                    await TryReplyAsync(
                        phoneNumberId,
                        message.From!,
                        "Could not read the image. Please try again.",
                        cancellationToken);
                    await MarkProcessedAsync(message.Id!, taskId: null, cancellationToken);
                    return;
                }

                var media = await _cloudApiClient.DownloadMediaAsync(
                    message.Image.Id,
                    cancellationToken);

                imageStream = new MemoryStream(media.Data);
                image = new ImageUploadRequest
                {
                    Content = imageStream,
                    FileName = media.FileName,
                    ContentType = media.ContentType,
                    Length = media.Data.Length
                };
            }

            if (string.IsNullOrWhiteSpace(feedbackText) && image is null)
            {
                await TryReplyAsync(
                    phoneNumberId,
                    message.From!,
                    "Please send a description and/or an image.",
                    cancellationToken);
                await MarkProcessedAsync(message.Id!, taskId: null, cancellationToken);
                return;
            }

            if (feedbackText.Length > TaskAnalysisLimits.FeedbackMaxLength)
            {
                feedbackText = feedbackText[..TaskAnalysisLimits.FeedbackMaxLength];
            }

            var command = new AnalyzeTaskCommand
            {
                ProjectId = session.SelectedProjectId,
                FeedbackText = feedbackText,
                Image = image,
                ActorUserId = botUser.Id,
                IsAdmin = true
            };

            var task = await _workflowService.AnalyzeAndCreateTaskAsync(
                command,
                cancellationToken);

            await _sessionRepository.DeleteAsync(
                phoneNumberId,
                message.From!,
                cancellationToken);

            await MarkProcessedAsync(message.Id!, task.Id, cancellationToken);

            await TryReplyAsync(
                phoneNumberId,
                message.From!,
                $"Task #{task.Id} created on *{session.SelectedProjectName}*: {task.Title}\n\n"
                + "Reply *menu* to create another task.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create task from WhatsApp message {MessageId} for project {ProjectId}.",
                message.Id,
                session.SelectedProjectId);

            await TryReplyAsync(
                phoneNumberId,
                message.From!,
                "Sorry, we could not create a task from that message. Please try again, or reply *menu* to restart.",
                cancellationToken);
            await MarkProcessedAsync(message.Id!, taskId: null, cancellationToken);
        }
        finally
        {
            if (imageStream is not null)
            {
                await imageStream.DisposeAsync();
            }
        }
    }

    private async Task SendProjectPickerAsync(
        string phoneNumberId,
        string recipientWaId,
        int page,
        CancellationToken cancellationToken)
    {
        var projects = (await _projectRepository.GetAllAsync())
            .OrderBy(p => p.Name)
            .ToList();

        if (projects.Count == 0)
        {
            await TryReplyAsync(
                phoneNumberId,
                recipientWaId,
                "No projects are available yet. Ask an admin to create a project first.",
                cancellationToken);
            return;
        }

        // Reserve one row for "More" when needed.
        var pageSize = projects.Count > MaxListRows ? MaxListRows - 1 : MaxListRows;
        var totalPages = (int)Math.Ceiling(projects.Count / (double)pageSize);
        page = Math.Clamp(page, 0, Math.Max(totalPages - 1, 0));

        var pageProjects = projects
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        var rows = pageProjects
            .Select(p => new WhatsAppListRow
            {
                Id = $"{ProjectRowPrefix}{p.Id}",
                Title = p.Name,
                Description = string.IsNullOrWhiteSpace(p.Description)
                    ? null
                    : p.Description
            })
            .ToList();

        if (page < totalPages - 1)
        {
            rows.Add(new WhatsAppListRow
            {
                Id = $"{PageRowPrefix}{page + 1}",
                Title = "More projects…",
                Description = $"Page {page + 2} of {totalPages}"
            });
        }
        else if (page > 0)
        {
            rows.Add(new WhatsAppListRow
            {
                Id = $"{PageRowPrefix}0",
                Title = "Back to first page",
                Description = $"Page 1 of {totalPages}"
            });
        }

        var section = new WhatsAppListSection
        {
            Title = "Projects",
            Rows = rows
        };

        var body = totalPages > 1
            ? $"Select a project for your task (page {page + 1}/{totalPages})."
            : "Select a project for your task.";

        try
        {
            if (!_whatsAppSettings.SendReplies)
            {
                return;
            }

            await _cloudApiClient.SendListMessageAsync(
                phoneNumberId,
                recipientWaId,
                body,
                "Projects",
                [section],
                headerText: "Create a task",
                footerText: "Reply cancel anytime to stop",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send WhatsApp project list to {Recipient}.", recipientWaId);

            var fallback = "Select a project by replying with its exact name:\n"
                + string.Join("\n", pageProjects.Select(p => $"• {p.Name}"));
            await TryReplyAsync(phoneNumberId, recipientWaId, fallback, cancellationToken);
        }
    }

    private async Task<WhatsAppConversationSession?> GetActiveSessionAsync(
        string phoneNumberId,
        string senderWaId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetAsync(
            phoneNumberId,
            senderWaId,
            cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.IsExpired())
        {
            await _sessionRepository.DeleteAsync(phoneNumberId, senderWaId, cancellationToken);
            return null;
        }

        return session;
    }

    private static bool IsMenuCommand(WhatsAppInboundMessage message)
    {
        if (!string.Equals(message.Type, "text", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var body = message.Text?.Body?.Trim();
        return !string.IsNullOrEmpty(body) && MenuCommands.Contains(body);
    }

    private async Task MarkProcessedAsync(
        string messageId,
        int? taskId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _processedMessageRepository.AddAsync(
                new ProcessedWhatsAppMessage(messageId, taskId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not persist processed WhatsApp message id {MessageId}.",
                messageId);
        }
    }

    private async Task TryReplyAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string body,
        CancellationToken cancellationToken)
    {
        if (!_whatsAppSettings.SendReplies)
        {
            return;
        }

        try
        {
            await _cloudApiClient.SendTextMessageAsync(
                businessPhoneNumberId,
                recipientWaId,
                body,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send WhatsApp reply to {Recipient}.",
                recipientWaId);
        }
    }
}
