using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.AIProviderService;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Infrastructure.AI;

public class OpenAiAiProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly string _systemPrompt;
    private readonly ILogger<OpenAiAiProvider> _logger;

    public OpenAiAiProvider(
        HttpClient httpClient,
        AISettings settings,
        ILogger<OpenAiAiProvider> logger)
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
        var userContent = BuildUserContent(request);

        var payload = new
        {
            model = _settings.Model,
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = _systemPrompt },
                new { role = "user", content = userContent }
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(payload)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _settings.ApiKey);

        using var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI request failed with status {StatusCode}.",
                (int)response.StatusCode);

            throw new InvalidOperationException(
                "AI analysis is temporarily unavailable. Please try again later.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var text = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return AiAnalysisResponseParser.Parse(text ?? string.Empty);
    }

    private static object BuildUserContent(TaskAnalysisRequest request)
    {
        var skillsJson = JsonSerializer.Serialize(
            request.AvailableSkills.Select(s => new { id = s.Id, name = s.Name }));

        var text = new StringBuilder()
            .AppendLine($"Project ID: {request.ProjectId}")
            .AppendLine("Feedback text:")
            .AppendLine(request.FeedbackText)
            .AppendLine("Available project skills (JSON array):")
            .AppendLine(skillsJson)
            .ToString();

        if (request.ImageContent is null || request.ImageContent.Data.Length == 0)
        {
            return text;
        }

        var dataUri =
            $"data:{request.ImageContent.ContentType};base64,{Convert.ToBase64String(request.ImageContent.Data)}";

        return new object[]
        {
            new { type = "text", text },
            new { type = "image_url", image_url = new { url = dataUri } }
        };
    }
}
