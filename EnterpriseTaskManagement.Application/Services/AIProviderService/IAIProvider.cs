using EnterpriseTaskManagement.Application.DTOs.AI;

namespace EnterpriseTaskManagement.Application.Services.AIProviderService;
    public interface IAIProvider
    {
        Task<TaskAnalysisResult> AnalyzeTaskAsync(
            TaskAnalysisRequest request,
            CancellationToken cancellationToken = default);
    }