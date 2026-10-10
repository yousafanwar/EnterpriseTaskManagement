using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.AIProviderService;

namespace EnterpriseTaskManagement.Infrastructure.AI;

public sealed class DisabledAiProvider : IAIProvider
{
    public Task<AiGeneratedTaskAnalysis> AnalyzeTaskAsync(TaskAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "AI task analysis is disabled. Set AI_ENABLED=true and configure AI credentials to use this feature.");
    }
}
