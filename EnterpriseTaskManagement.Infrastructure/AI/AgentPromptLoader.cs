namespace EnterpriseTaskManagement.Infrastructure.AI;

public static class AgentPromptLoader
{
    public static string Load(string agentPromptPath)
    {
        if (string.IsNullOrWhiteSpace(agentPromptPath))
        {
            throw new InvalidOperationException(
                "AI agent prompt path is not configured.");
        }

        if (!File.Exists(agentPromptPath))
        {
            throw new FileNotFoundException(
                "AI agent prompt file was not found.",
                agentPromptPath);
        }

        return File.ReadAllText(agentPromptPath);
    }
}
