namespace EnterpriseTaskManagement.Domain.Entities;

public enum WhatsAppConversationStep
{
    AwaitingFeedback = 1
}

/// <summary>
/// Tracks multi-step WhatsApp task creation for a sender on a business line.
/// </summary>
public class WhatsAppConversationSession
{
    public const int DefaultTtlMinutes = 60;

    public int Id { get; private set; }

    /// <summary>
    /// Meta Cloud API business <c>phone_number_id</c>.
    /// </summary>
    public string BusinessPhoneNumberId { get; private set; } = null!;

    /// <summary>
    /// Sender WhatsApp id (<c>from</c> on inbound messages).
    /// </summary>
    public string SenderWaId { get; private set; } = null!;

    public WhatsAppConversationStep Step { get; private set; }

    public int SelectedProjectId { get; private set; }

    public string SelectedProjectName { get; private set; } = null!;

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    private WhatsAppConversationSession()
    {
    }

    public WhatsAppConversationSession(
        string businessPhoneNumberId,
        string senderWaId,
        int selectedProjectId,
        string selectedProjectName,
        TimeSpan? ttl = null)
    {
        BusinessPhoneNumberId = businessPhoneNumberId.Trim();
        SenderWaId = senderWaId.Trim();
        SelectedProjectId = selectedProjectId;
        SelectedProjectName = selectedProjectName.Trim();
        Step = WhatsAppConversationStep.AwaitingFeedback;
        Touch(ttl);
    }

    public bool IsExpired(DateTime? utcNow = null)
    {
        return ExpiresAtUtc <= (utcNow ?? DateTime.UtcNow);
    }

    public void SelectProject(int projectId, string projectName, TimeSpan? ttl = null)
    {
        SelectedProjectId = projectId;
        SelectedProjectName = projectName.Trim();
        Step = WhatsAppConversationStep.AwaitingFeedback;
        Touch(ttl);
    }

    public void Touch(TimeSpan? ttl = null)
    {
        var lifetime = ttl ?? TimeSpan.FromMinutes(DefaultTtlMinutes);
        UpdatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = UpdatedAtUtc.Add(lifetime);
    }
}
