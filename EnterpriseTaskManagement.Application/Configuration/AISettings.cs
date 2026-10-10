namespace EnterpriseTaskManagement.Application.Configuration;

public class AISettings
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Gemini";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string AgentPromptPath { get; set; } = string.Empty;
}
