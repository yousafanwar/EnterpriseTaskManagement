using EnterpriseTaskManagement.Application.Services.WhatsApp;

namespace EnterpriseTaskManagement.Infrastructure.WhatsApp;

public class DisabledWhatsAppCloudApiClient : IWhatsAppCloudApiClient
{
    public Task<WhatsAppMediaDownloadResult> DownloadMediaAsync(
        string mediaId,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "WhatsApp integration is disabled. Set WHATSAPP_ENABLED=true and configure credentials.");
    }

    public Task SendTextMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string body,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "WhatsApp integration is disabled. Set WHATSAPP_ENABLED=true and configure credentials.");
    }

    public Task SendListMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string bodyText,
        string buttonText,
        IReadOnlyList<WhatsAppListSection> sections,
        string? headerText = null,
        string? footerText = null,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "WhatsApp integration is disabled. Set WHATSAPP_ENABLED=true and configure credentials.");
    }
}
