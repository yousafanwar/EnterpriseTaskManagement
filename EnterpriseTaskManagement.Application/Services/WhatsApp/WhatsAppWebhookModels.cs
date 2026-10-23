using System.Text.Json.Serialization;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public class WhatsAppWebhookPayload
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("entry")]
    public List<WhatsAppWebhookEntry>? Entry { get; set; }
}

public class WhatsAppWebhookEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("changes")]
    public List<WhatsAppWebhookChange>? Changes { get; set; }
}

public class WhatsAppWebhookChange
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("value")]
    public WhatsAppWebhookValue? Value { get; set; }
}

public class WhatsAppWebhookValue
{
    [JsonPropertyName("messaging_product")]
    public string? MessagingProduct { get; set; }

    [JsonPropertyName("metadata")]
    public WhatsAppWebhookMetadata? Metadata { get; set; }

    [JsonPropertyName("messages")]
    public List<WhatsAppInboundMessage>? Messages { get; set; }

    [JsonPropertyName("statuses")]
    public List<object>? Statuses { get; set; }
}

public class WhatsAppWebhookMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string? DisplayPhoneNumber { get; set; }

    [JsonPropertyName("phone_number_id")]
    public string? PhoneNumberId { get; set; }
}

public class WhatsAppInboundMessage
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public WhatsAppTextBody? Text { get; set; }

    [JsonPropertyName("image")]
    public WhatsAppImageBody? Image { get; set; }
}

public class WhatsAppTextBody
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class WhatsAppImageBody
{
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
