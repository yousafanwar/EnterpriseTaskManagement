
namespace EnterpriseTaskManagement.Application.DTOs.AI;

public class TaskAnalysisRequest
{
    public int ProjectId { get; set; }

    public string FeedbackText { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }

    public TaskAnalysisImageContent? ImageContent { get; set; }

    public IReadOnlyList<TaskAnalysisSkill> AvailableSkills { get; set; } = Array.Empty<TaskAnalysisSkill>();
}

public class TaskAnalysisImageContent
{
    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string ContentType { get; init; } = string.Empty;
}

public class TaskAnalysisSkill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
