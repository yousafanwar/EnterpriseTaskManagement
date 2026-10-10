using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.API.Contracts.AI;

public class AnalyzeTaskFormRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [StringLength(4000)]
    public string FeedbackText { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}
