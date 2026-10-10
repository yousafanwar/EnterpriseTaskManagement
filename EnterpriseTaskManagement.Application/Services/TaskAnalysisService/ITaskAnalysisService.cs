using EnterpriseTaskManagement.Application.DTOs.AI;

namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;
public interface ITaskAnalysisService
{
    Task<TaskAnalysisResult> AnalyzeFeedbackAsync(TaskAnalysisRequest request, CancellationToken cancellationToken = default);
}