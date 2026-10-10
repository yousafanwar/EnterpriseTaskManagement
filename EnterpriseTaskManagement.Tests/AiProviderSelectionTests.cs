using EnterpriseTaskManagement.Infrastructure.DependencyInjection;

namespace EnterpriseTaskManagement.Tests;

public class AiProviderSelectionTests
{
    [Theory]
    [InlineData("Gemini", true)]
    [InlineData("openai", true)]
    [InlineData("Anthropic", false)]
    public void IsSupportedAiProvider_MatchesExpected(string provider, bool expected)
    {
        Assert.Equal(expected, ServiceCollectionExtensions.IsSupportedAiProvider(provider));
    }
}
