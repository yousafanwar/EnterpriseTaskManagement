using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Services.WhatsApp;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Infrastructure.WhatsApp;

public class WhatsAppCloudApiClient : IWhatsAppCloudApiClient
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WhatsAppCloudApiClient> _logger;

    public WhatsAppCloudApiClient(
        HttpClient httpClient,
        WhatsAppSettings settings,
        ILogger<WhatsAppCloudApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<WhatsAppMediaDownloadResult> DownloadMediaAsync(
        string mediaId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media id is required.", nameof(mediaId));
        }

        using var metaRequest = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"{_settings.GraphApiBaseUrl.TrimEnd('/')}/{mediaId}");

        using var metaResponse = await _httpClient.SendAsync(metaRequest, cancellationToken);
        if (!metaResponse.IsSuccessStatusCode)
        {
            var errorBody = await metaResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "WhatsApp media metadata request failed with {StatusCode}: {Body}",
                (int)metaResponse.StatusCode,
                errorBody);
            throw new InvalidOperationException(
                "Failed to retrieve WhatsApp media metadata.");
        }

        var metadata = await metaResponse.Content.ReadFromJsonAsync<WhatsAppMediaMetadata>(
            cancellationToken: cancellationToken);

        if (metadata is null || string.IsNullOrWhiteSpace(metadata.Url))
        {
            throw new InvalidOperationException("WhatsApp media metadata did not include a download URL.");
        }

        using var downloadRequest = CreateAuthorizedRequest(HttpMethod.Get, metadata.Url);
        using var downloadResponse = await _httpClient.SendAsync(
            downloadRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!downloadResponse.IsSuccessStatusCode)
        {
            var errorBody = await downloadResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "WhatsApp media download failed with {StatusCode}: {Body}",
                (int)downloadResponse.StatusCode,
                errorBody);
            throw new InvalidOperationException("Failed to download WhatsApp media.");
        }

        var data = await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = metadata.MimeType
            ?? downloadResponse.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".bin"
        };

        return new WhatsAppMediaDownloadResult
        {
            Data = data,
            ContentType = contentType,
            FileName = $"whatsapp-{mediaId}{extension}"
        };
    }

    public async Task SendTextMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumberId))
        {
            throw new ArgumentException("Business phone number id is required.", nameof(businessPhoneNumberId));
        }

        if (string.IsNullOrWhiteSpace(recipientWaId))
        {
            throw new ArgumentException("Recipient WhatsApp id is required.", nameof(recipientWaId));
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = recipientWaId,
            type = "text",
            text = new { preview_url = false, body }
        };

        await SendMessagePayloadAsync(businessPhoneNumberId, payload, cancellationToken);
    }

    public async Task SendListMessageAsync(
        string businessPhoneNumberId,
        string recipientWaId,
        string bodyText,
        string buttonText,
        IReadOnlyList<WhatsAppListSection> sections,
        string? headerText = null,
        string? footerText = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessPhoneNumberId))
        {
            throw new ArgumentException("Business phone number id is required.", nameof(businessPhoneNumberId));
        }

        if (string.IsNullOrWhiteSpace(recipientWaId))
        {
            throw new ArgumentException("Recipient WhatsApp id is required.", nameof(recipientWaId));
        }

        if (sections is null || sections.Count == 0)
        {
            throw new ArgumentException("At least one list section is required.", nameof(sections));
        }

        var interactive = new Dictionary<string, object?>
        {
            ["type"] = "list",
            ["body"] = new { text = bodyText },
            ["action"] = new
            {
                button = Truncate(buttonText, 20),
                sections = sections.Select(section => new
                {
                    title = Truncate(section.Title, 24),
                    rows = section.Rows.Select(row =>
                    {
                        var rowPayload = new Dictionary<string, object?>
                        {
                            ["id"] = Truncate(row.Id, 200),
                            ["title"] = Truncate(row.Title, 24)
                        };

                        if (!string.IsNullOrWhiteSpace(row.Description))
                        {
                            rowPayload["description"] = Truncate(row.Description, 72);
                        }

                        return rowPayload;
                    }).ToList()
                }).ToList()
            }
        };

        if (!string.IsNullOrWhiteSpace(headerText))
        {
            interactive["header"] = new { type = "text", text = Truncate(headerText, 60) };
        }

        if (!string.IsNullOrWhiteSpace(footerText))
        {
            interactive["footer"] = new { text = Truncate(footerText, 60) };
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = recipientWaId,
            type = "interactive",
            interactive
        };

        await SendMessagePayloadAsync(businessPhoneNumberId, payload, cancellationToken);
    }

    private async Task SendMessagePayloadAsync(
        string businessPhoneNumberId,
        object payload,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"{_settings.GraphApiBaseUrl.TrimEnd('/')}/{businessPhoneNumberId}/messages");
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "WhatsApp send message failed with {StatusCode}: {Body}",
                (int)response.StatusCode,
                errorBody);
            throw new InvalidOperationException("Failed to send WhatsApp message.");
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        return request;
    }

    private sealed class WhatsAppMediaMetadata
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }
    }
}
