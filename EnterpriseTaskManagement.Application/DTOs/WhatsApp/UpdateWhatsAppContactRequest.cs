using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.WhatsApp;

public class UpdateWhatsAppContactRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [StringLength(200)]
    public string? Label { get; set; }

    public bool IsActive { get; set; } = true;
}
