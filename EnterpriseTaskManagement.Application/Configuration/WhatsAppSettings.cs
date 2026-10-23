namespace EnterpriseTaskManagement.Application.Configuration;

public class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";

    public const string BotEmail = "whatsapp-bot@system.local";

    public const string BotFirstName = "WhatsApp";

    public const string BotLastName = "Bot";

    public bool Enabled { get; set; }

    public string VerifyToken { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com/v21.0";

    /// <summary>
    /// When true, send a WhatsApp text reply after a task is created (or on failure).
    /// </summary>
    public bool SendReplies { get; set; } = true;
}
