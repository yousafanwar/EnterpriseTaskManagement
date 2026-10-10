namespace EnterpriseTaskManagement.Application.DTOs.AI;

public class TaskAnalysisResult
{
    // AI generated fields
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<int> RequiredSkillIds { get; set; } = Array.Empty<int>();
    
    // Original user provided feedback
    public string FeedbackText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
}
