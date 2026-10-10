using System.Text.Json;
using EnterpriseTaskManagement.Application.DTOs.AI;

namespace EnterpriseTaskManagement.Infrastructure.AI;

public static class AiAnalysisResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AiGeneratedTaskAnalysis Parse(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }

        var json = ExtractJsonObject(rawContent);

        AiGeneratedTaskAnalysis? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AiGeneratedTaskAnalysis>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "AI returned malformed JSON.",
                ex);
        }

        if (parsed is null)
        {
            throw new InvalidOperationException("AI returned malformed JSON.");
        }

        parsed.RequiredSkillIds ??= Array.Empty<int>();

        return parsed;
    }

    private static string ExtractJsonObject(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                trimmed = trimmed[(firstNewLine + 1)..];
            }

            var fenceIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceIndex >= 0)
            {
                trimmed = trimmed[..fenceIndex];
            }

            trimmed = trimmed.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new InvalidOperationException("AI response did not contain a JSON object.");
        }

        return trimmed[start..(end + 1)];
    }
}
