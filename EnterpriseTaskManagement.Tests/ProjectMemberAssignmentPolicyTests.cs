using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

namespace EnterpriseTaskManagement.Tests;

public class ProjectMemberAssignmentPolicyTests
{
    [Fact]
    public void SelectAssignee_RequiresAtLeastOneMatchingSkill()
    {
        var userSkills = new Dictionary<int, HashSet<int>>
        {
            [10] = [1, 2],
            [20] = [3],
            [30] = [2, 4]
        };

        var assignee = ProjectMemberAssignmentPolicy.SelectAssignee(
            [10, 20, 30],
            [2, 5],
            userSkills);

        Assert.Equal(10, assignee);
    }

    [Fact]
    public void SelectAssignee_WhenNoSkillsRequired_PicksLowestMemberId()
    {
        var assignee = ProjectMemberAssignmentPolicy.SelectAssignee(
            [30, 10, 20],
            [],
            new Dictionary<int, HashSet<int>>());

        Assert.Equal(10, assignee);
    }

    [Fact]
    public void SelectAssignee_ReturnsNullWhenNoMemberMatches()
    {
        var assignee = ProjectMemberAssignmentPolicy.SelectAssignee(
            [10, 20],
            [99],
            new Dictionary<int, HashSet<int>>
            {
                [10] = [1],
                [20] = [2]
            });

        Assert.Null(assignee);
    }
}
