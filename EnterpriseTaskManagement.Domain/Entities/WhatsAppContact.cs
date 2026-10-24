namespace EnterpriseTaskManagement.Domain.Entities;

/// <summary>
/// Enables a WhatsApp Business phone number for inbound task creation.
/// Project selection happens in the chat flow; <see cref="ProjectId"/> is retained for admin labeling only.
/// </summary>
public class WhatsAppContact
{
    public int Id { get; private set; }

    /// <summary>
    /// Meta Cloud API <c>phone_number_id</c> from webhook metadata for the business line.
    /// </summary>
    public string PhoneNumberId { get; private set; } = null!;

    /// <summary>
    /// Optional admin reference project; not used to route inbound messages.
    /// </summary>
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
