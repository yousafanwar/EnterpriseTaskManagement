namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public interface IWhatsAppCloudApiClient
{
    Task<WhatsAppMediaDownloadResult> DownloadMediaAsync(
        string mediaId,
        CancellationToken cancellationToken = default);

    Task SendTextMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string body,
        CancellationToken cancellationToken = default);

    Task SendListMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string bodyText,
        string buttonText,
        IReadOnlyList<WhatsAppListSection> sections,
        string? headerText = null,
        string? footerText = null,
        CancellationToken cancellationToken = default);
}

public class WhatsAppMediaDownloadResult
{
    public required byte[] Data { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}
