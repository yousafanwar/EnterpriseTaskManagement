namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

/// <summary>
/// Assignment policy: a project member qualifies if they have at least one of the required skills.
/// When no skills are required, any project member qualifies.
/// Tie-break: highest count of matching required skills, then lowest user id.
/// </summary>
public static class ProjectMemberAssignmentPolicy
{
    public static int? SelectAssignee(
        IReadOnlyList<int> projectMemberUserIds,
        IReadOnlyList<int> requiredSkillIds,
        IReadOnlyDictionary<int, HashSet<int>> userSkillIdsByUser)
    {
        if (projectMemberUserIds.Count == 0)
        {
            return null;
        }

        var required = requiredSkillIds.Count == 0
            ? null
            : requiredSkillIds.ToHashSet();

        int? bestUserId = null;
        var bestMatchCount = -1;

        foreach (var userId in projectMemberUserIds.OrderBy(id => id))
        {
            if (!userSkillIdsByUser.TryGetValue(userId, out var userSkills))
            {
                userSkills = [];
            }

            if (required is not null)
            {
                var matchCount = userSkills.Count(required.Contains);
                if (matchCount == 0)
                {
                    continue;
                }

                if (matchCount > bestMatchCount
                    || (matchCount == bestMatchCount && bestUserId is null))
                {
                    bestMatchCount = matchCount;
                    bestUserId = userId;
                }
            }
            else if (bestUserId is null)
            {
                bestUserId = userId;
                bestMatchCount = 0;
            }
        }

        return bestUserId;
    }
}
