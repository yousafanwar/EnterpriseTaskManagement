using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.WhatsApp;

public class CreateWhatsAppContactRequest
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string PhoneNumberId { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    [StringLength(200)]
    public string? Label { get; set; }
}
