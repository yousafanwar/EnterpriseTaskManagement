using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.AIProviderService;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

public class TaskAnalysisService : ITaskAnalysisService
{
    private readonly IAIProvider _aiProvider;
    private readonly ILogger<TaskAnalysisService> _logger;

    public TaskAnalysisService(
        IAIProvider aiProvider,
        ILogger<TaskAnalysisService> logger)
    {
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<TaskAnalysisResult> AnalyzeFeedbackAsync(
        TaskAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var generated = await _aiProvider.AnalyzeTaskAsync(request, cancellationToken);

        var projectSkillIds = request.AvailableSkills.Select(s => s.Id).ToList();
        var validatedSkillIds = SkillIdValidation.FilterToProjectSkills(
            generated.RequiredSkillIds,
            projectSkillIds);

        if (validatedSkillIds.Count != generated.RequiredSkillIds.Count)
        {
            _logger.LogWarning(
                "AI returned skill ids outside the project skill set for project {ProjectId}.",
                request.ProjectId);
        }

        var title = NormalizeTitle(generated.Title);
        var description = NormalizeDescription(generated.Description);

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException(
                "The feedback did not contain enough information to create a task.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = Truncate(description, TaskAnalysisLimits.TitleMaxLength);
        }

        return new TaskAnalysisResult
        {
            Title = title,
            Description = description,
            RequiredSkillIds = validatedSkillIds,
            FeedbackText = request.FeedbackText,
            ImageUrl = request.ImageUrl,
            ImagePublicId = request.ImagePublicId
        };
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title.Trim();
        return Truncate(trimmed, TaskAnalysisLimits.TitleMaxLength);
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        return Truncate(trimmed, TaskAnalysisLimits.DescriptionMaxLength);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
