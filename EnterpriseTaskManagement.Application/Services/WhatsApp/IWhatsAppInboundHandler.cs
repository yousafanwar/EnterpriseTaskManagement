namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public interface IWhatsAppInboundHandler
{
    Task HandleWebhookPayloadAsync(
        string rawJsonBody,
        CancellationToken cancellationToken = default);
}
