using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.AIProviderService;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Infrastructure.AI;

public class GeminiAiProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly string _systemPrompt;
    private readonly ILogger<GeminiAiProvider> _logger;

    public GeminiAiProvider(
        HttpClient httpClient,
        AISettings settings,
        ILogger<GeminiAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
        _systemPrompt = AgentPromptLoader.Load(_settings.AgentPromptPath);
    }

    public async Task<AiGeneratedTaskAnalysis> AnalyzeTaskAsync(
        TaskAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var userMessage = BuildUserMessage(request);
        var parts = new List<object> { new { text = userMessage } };

        if (request.ImageContent is not null && request.ImageContent.Data.Length > 0)
        {
            parts.Add(new
            {
                inlineData = new
                {
                    mimeType = request.ImageContent.ContentType,
                    data = Convert.ToBase64String(request.ImageContent.Data)
                }
            });
        }

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = _systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.2
            }
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(_settings.Model)}:generateContent?key={Uri.EscapeDataString(_settings.ApiKey)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(
                payload,
                options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
        };

        using var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Gemini request failed with status {StatusCode}.",
                (int)response.StatusCode);

            throw new InvalidOperationException(
                "AI analysis is temporarily unavailable. Please try again later.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return AiAnalysisResponseParser.Parse(text ?? string.Empty);
    }

    private static string BuildUserMessage(TaskAnalysisRequest request)
    {
        var skillsJson = JsonSerializer.Serialize(
            request.AvailableSkills.Select(s => new { id = s.Id, name = s.Name }));

        var builder = new StringBuilder();
        builder.AppendLine($"Project ID: {request.ProjectId}");
        builder.AppendLine("Feedback text:");
        builder.AppendLine(request.FeedbackText);
        builder.AppendLine("Available project skills (JSON array):");
        builder.AppendLine(skillsJson);

        return builder.ToString();
    }
}
