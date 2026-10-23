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
}

public class WhatsAppMediaDownloadResult
{
    public required byte[] Data { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}
