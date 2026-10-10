using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services.ImageStorage;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

public class AiTaskWorkflowService : IAiTaskWorkflowService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectUserRepository _projectUserRepository;
    private readonly IProjectSkillRepository _projectSkillRepository;
    private readonly IUserSkillRepository _userSkillRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITaskHistoryService _taskHistoryService;
    private readonly ITaskAnalysisService _taskAnalysisService;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<AiTaskWorkflowService> _logger;

    public AiTaskWorkflowService(
        IProjectRepository projectRepository,
        IProjectUserRepository projectUserRepository,
        IProjectSkillRepository projectSkillRepository,
        IUserSkillRepository userSkillRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        ITaskHistoryService taskHistoryService,
        ITaskAnalysisService taskAnalysisService,
        IImageStorageService imageStorageService,
        ILogger<AiTaskWorkflowService> logger)
    {
        _projectRepository = projectRepository;
        _projectUserRepository = projectUserRepository;
        _projectSkillRepository = projectSkillRepository;
        _userSkillRepository = userSkillRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _taskHistoryService = taskHistoryService;
        _taskAnalysisService = taskAnalysisService;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<TaskItemDto> AnalyzeAndCreateTaskAsync(
        AnalyzeTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var project = await _projectRepository.GetByIdAsync(command.ProjectId);
        if (project is null)
        {
            throw new KeyNotFoundException(
                $"Project with id {command.ProjectId} was not found.");
        }

        await EnsureProjectAccessAsync(command);

        ImageUploadResult? uploadedImage = null;
        TaskAnalysisImageContent? imageContent = null;
        try
        {
            if (command.Image is not null)
            {
                imageContent = await ReadImageContentAsync(command.Image, cancellationToken);
                var uploadRequest = new ImageUploadRequest
                {
                    Content = new MemoryStream(imageContent.Data),
                    FileName = command.Image.FileName,
                    ContentType = imageContent.ContentType,
                    Length = imageContent.Data.Length
                };
                uploadedImage = await _imageStorageService.UploadAsync(
                    uploadRequest,
                    cancellationToken);
            }

            var projectSkills = await _projectSkillRepository.GetByProjectIdAsync(
                command.ProjectId);

            var analysisRequest = new TaskAnalysisRequest
            {
                ProjectId = command.ProjectId,
                FeedbackText = command.FeedbackText.Trim(),
                ImageUrl = uploadedImage?.Url,
                ImagePublicId = uploadedImage?.PublicId,
                ImageContent = imageContent,
                AvailableSkills = projectSkills
                    .Select(ps => new TaskAnalysisSkill
                    {
                        Id = ps.SkillId,
                        Name = ps.Skill.Name
                    })
                    .ToList()
            };

            var analysis = await _taskAnalysisService.AnalyzeFeedbackAsync(
                analysisRequest,
                cancellationToken);

            var assigneeId = await ResolveAssigneeAsync(
                command.ProjectId,
                analysis.RequiredSkillIds,
                cancellationToken);

            var task = new TaskItem(
                analysis.Title,
                analysis.Description,
                command.ProjectId,
                analysis.FeedbackText,
                analysis.ImageUrl,
                analysis.ImagePublicId);

            if (assigneeId.HasValue)
            {
                task.AssignTo(assigneeId.Value);
            }

            await _taskRepository.AddTaskWithRequiredSkillsAsync(
                task,
                analysis.RequiredSkillIds,
                cancellationToken);

            await _taskHistoryService.AddHistoryAsync(
                task.Id,
                command.ActorUserId,
                "Created");

            if (assigneeId.HasValue)
            {
                await _taskHistoryService.AddHistoryAsync(
                    task.Id,
                    command.ActorUserId,
                    "Assigned",
                    null,
                    assigneeId.Value.ToString());
            }

            _logger.LogInformation(
                "AI task {TaskId} created for project {ProjectId} by user {ActorUserId}.",
                task.Id,
                command.ProjectId,
                command.ActorUserId);

            return await MapToDtoAsync(task, project.Name, assigneeId, analysis.RequiredSkillIds);
        }
        catch
        {
            if (uploadedImage is not null)
            {
                try
                {
                    await _imageStorageService.DeleteAsync(
                        uploadedImage.PublicId,
                        cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(
                        cleanupEx,
                        "Failed to delete orphaned Cloudinary asset {PublicId}.",
                        uploadedImage.PublicId);
                }
            }

            throw;
        }
    }

    private static async Task<TaskAnalysisImageContent> ReadImageContentAsync(
        ImageUploadRequest image,
        CancellationToken cancellationToken)
    {
        if (image.Content.CanSeek)
        {
            image.Content.Position = 0;
        }

        using var memory = new MemoryStream();
        await image.Content.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        if (!ImageContentValidator.TryDetectContentType(memory, out var contentType)
            || !ImageContentValidator.IsAllowedContentType(contentType))
        {
            throw new ArgumentException(
                "Unsupported or invalid image format. Allowed formats: JPEG, PNG, GIF, WebP.");
        }

        return new TaskAnalysisImageContent
        {
            Data = memory.ToArray(),
            ContentType = contentType
        };
    }

    private static void ValidateCommand(AnalyzeTaskCommand command)
    {
        if (command.ProjectId <= 0)
        {
            throw new ArgumentException(
                "ProjectId must be a valid project id.",
                nameof(command.ProjectId));
        }

        var feedback = command.FeedbackText?.Trim() ?? string.Empty;
        if (feedback.Length > TaskAnalysisLimits.FeedbackMaxLength)
        {
            throw new ArgumentException(
                $"FeedbackText cannot exceed {TaskAnalysisLimits.FeedbackMaxLength} characters.",
                nameof(command.FeedbackText));
        }

        if (string.IsNullOrWhiteSpace(feedback) && command.Image is null)
        {
            throw new ArgumentException(
                "FeedbackText or an image is required.",
                nameof(command.FeedbackText));
        }
    }

    private async Task EnsureProjectAccessAsync(AnalyzeTaskCommand command)
    {
        if (command.IsAdmin)
        {
            return;
        }

        var membership = await _projectUserRepository.GetAsync(
            command.ProjectId,
            command.ActorUserId);

        if (membership is null)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this project.");
        }
    }

    private async Task<int?> ResolveAssigneeAsync(
        int projectId,
        IReadOnlyList<int> requiredSkillIds,
        CancellationToken cancellationToken)
    {
        var projectUsers = await _projectUserRepository.GetByProjectIdAsync(projectId);
        var memberIds = projectUsers.Select(pu => pu.UserId).ToList();

        var userSkills = await _userSkillRepository.GetSkillIdsByUserIdsAsync(
            memberIds,
            cancellationToken);

        return ProjectMemberAssignmentPolicy.SelectAssignee(
            memberIds,
            requiredSkillIds,
            userSkills);
    }

    private async Task<TaskItemDto> MapToDtoAsync(
        TaskItem task,
        string projectName,
        int? assigneeId,
        IReadOnlyList<int> requiredSkillIds)
    {
        AssigneeDto? assignee = null;
        if (assigneeId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(assigneeId.Value);
            if (user is not null)
            {
                assignee = new AssigneeDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
            }
        }

        return new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            ProjectId = task.ProjectId,
            ProjectName = projectName,
            Assignee = assignee,
            OriginalFeedback = task.OriginalFeedback,
            FeedbackImageUrl = task.FeedbackImageUrl,
            RequiredSkillIds = requiredSkillIds
        };
    }
}
