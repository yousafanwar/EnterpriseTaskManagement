namespace EnterpriseTaskManagement.Application.DTOs.AI;

public class AiGeneratedTaskAnalysis
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<int> RequiredSkillIds { get; set; } = Array.Empty<int>();
}
