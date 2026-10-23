namespace EnterpriseTaskManagement.Domain.Entities;

/// <summary>
/// Stores WhatsApp message ids so webhook retries do not create duplicate tasks.
/// </summary>
public class ProcessedWhatsAppMessage
{
    public int Id { get; private set; }

    public string MessageId { get; private set; } = null!;

    public int? TaskId { get; private set; }

    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedWhatsAppMessage()
    {
    }

    public ProcessedWhatsAppMessage(string messageId, int? taskId)
    {
        MessageId = messageId;
        TaskId = taskId;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
