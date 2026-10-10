using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

namespace EnterpriseTaskManagement.Tests;

public class SkillIdValidationTests
{
    [Fact]
    public void FilterToProjectSkills_RemovesUnknownAndDuplicateIds()
    {
        var projectSkills = new[] { 1, 2, 3 };

        var result = SkillIdValidation.FilterToProjectSkills(
            [2, 99, 2, 1, 1],
            projectSkills);

        Assert.Equal([2, 1], result);
    }
}
