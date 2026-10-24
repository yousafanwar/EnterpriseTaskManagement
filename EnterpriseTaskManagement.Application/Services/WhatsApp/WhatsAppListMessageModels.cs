namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public class WhatsAppListSection
{
    public required string Title { get; init; }

    public required IReadOnlyList<WhatsAppListRow> Rows { get; init; }
}

public class WhatsAppListRow
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }
}
