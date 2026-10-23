namespace EnterpriseTaskManagement.Domain.Entities;

/// <summary>
/// Maps a WhatsApp Business phone number (one chat/line per project) to a fixed project.
/// </summary>
public class WhatsAppContact
{
    public int Id { get; private set; }

    /// <summary>
    /// Meta Cloud API <c>phone_number_id</c> from webhook metadata for the business line.
    /// </summary>
    public string PhoneNumberId { get; private set; } = null!;

    public int ProjectId { get; private set; }

    public Project Project { get; private set; } = null!;

    public string? Label { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private WhatsAppContact()
    {
    }

    public WhatsAppContact(string phoneNumberId, int projectId, string? label = null)
    {
        PhoneNumberId = phoneNumberId.Trim();
        ProjectId = projectId;
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(int projectId, string? label, bool isActive)
    {
        ProjectId = projectId;
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        IsActive = isActive;
    }
}
