using EnterpriseTaskManagement.Infrastructure.AI;

namespace EnterpriseTaskManagement.Tests;

public class AiAnalysisResponseParserTests
{
    [Fact]
    public void Parse_ReadsJsonFromMarkdownFence()
    {
        var raw = """
            ```json
            {
              "title": "Fix login button",
              "description": "Button does not respond on mobile.",
              "requiredSkillIds": [3, 3, 8]
            }
            ```
            """;

        var result = AiAnalysisResponseParser.Parse(raw);

        Assert.Equal("Fix login button", result.Title);
        Assert.Equal([3, 3, 8], result.RequiredSkillIds);
    }

    [Fact]
    public void Parse_ThrowsForMalformedJson()
    {
        Assert.Throws<InvalidOperationException>(
            () => AiAnalysisResponseParser.Parse("{ not-json"));
    }
}
