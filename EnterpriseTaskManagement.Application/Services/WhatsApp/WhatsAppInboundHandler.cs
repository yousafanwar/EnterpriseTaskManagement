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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWhatsAppContactRepository _contactRepository;
    private readonly IProcessedWhatsAppMessageRepository _processedMessageRepository;
    private readonly IWhatsAppCloudApiClient _cloudApiClient;
    private readonly IAiTaskWorkflowService _workflowService;
    private readonly IUserRepository _userRepository;
    private readonly AISettings _aiSettings;
    private readonly WhatsAppSettings _whatsAppSettings;
    private readonly ILogger<WhatsAppInboundHandler> _logger;

    public WhatsAppInboundHandler(
        IWhatsAppContactRepository contactRepository,
        IProcessedWhatsAppMessageRepository processedMessageRepository,
        IWhatsAppCloudApiClient cloudApiClient,
        IAiTaskWorkflowService workflowService,
        IUserRepository userRepository,
        AISettings aiSettings,
        WhatsAppSettings whatsAppSettings,
        ILogger<WhatsAppInboundHandler> logger)
    {
        _contactRepository = contactRepository;
        _processedMessageRepository = processedMessageRepository;
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
        if (messageType is not ("text" or "image"))
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
                "This WhatsApp number is not linked to a project. Ask an admin to configure it.",
                cancellationToken);
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

        var botUser = await _userRepository.GetByEmailAsync(WhatsAppSettings.BotEmail);
        if (botUser is null)
        {
            _logger.LogError(
                "WhatsApp bot user {Email} was not found. Cannot create task history.",
                WhatsAppSettings.BotEmail);
            await TryReplyAsync(
                phoneNumberId,
                message.From,
                "WhatsApp bot is not configured. Contact an administrator.",
                cancellationToken);
            return;
        }

        string feedbackText;
        ImageUploadRequest? image = null;
        MemoryStream? imageStream = null;

        try
        {
            if (messageType == "text")
            {
                feedbackText = message.Text?.Body?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(feedbackText))
                {
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
                        message.From,
                        "Could not read the image. Please try again.",
                        cancellationToken);
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

            if (feedbackText.Length > TaskAnalysisLimits.FeedbackMaxLength)
            {
                feedbackText = feedbackText[..TaskAnalysisLimits.FeedbackMaxLength];
            }

            var command = new AnalyzeTaskCommand
            {
                ProjectId = contact.ProjectId,
                FeedbackText = feedbackText,
                Image = image,
                ActorUserId = botUser.Id,
                IsAdmin = true
            };

            var task = await _workflowService.AnalyzeAndCreateTaskAsync(
                command,
                cancellationToken);

            await MarkProcessedAsync(message.Id, task.Id, cancellationToken);

            await TryReplyAsync(
                phoneNumberId,
                message.From,
                $"Task #{task.Id} created: {task.Title}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create task from WhatsApp message {MessageId} for project {ProjectId}.",
                message.Id,
                contact.ProjectId);

            await TryReplyAsync(
                phoneNumberId,
                message.From,
                "Sorry, we could not create a task from that message. Please try again.",
                cancellationToken);
        }
        finally
        {
            if (imageStream is not null)
            {
                await imageStream.DisposeAsync();
            }
        }
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
            // Likely a unique-index race on webhook retry; safe to ignore after create.
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
