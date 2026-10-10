using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.DTOs.AI;

namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

public interface IAiTaskWorkflowService
{
    Task<TaskItemDto> AnalyzeAndCreateTaskAsync(
        AnalyzeTaskCommand command,
        CancellationToken cancellationToken = default);
}

public class AnalyzeTaskCommand
{
    public int ProjectId { get; init; }

    public string FeedbackText { get; init; } = string.Empty;

    public ImageUploadRequest? Image { get; init; }

    public int ActorUserId { get; init; }

    public bool IsAdmin { get; init; }
}
