namespace EnterpriseTaskManagement.Application.DTOs.WhatsApp;

public class WhatsAppContactDto
{
    public int Id { get; set; }

    public string PhoneNumberId { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public string? ProjectName { get; set; }

    public string? Label { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
