namespace EnterpriseTaskManagement.Application.Services.TaskAnalysisService;

public static class SkillIdValidation
{
    public static IReadOnlyList<int> FilterToProjectSkills(
        IEnumerable<int> candidateSkillIds,
        IReadOnlyCollection<int> projectSkillIds)
    {
        var allowed = projectSkillIds as HashSet<int> ?? projectSkillIds.ToHashSet();
        var seen = new HashSet<int>();
        var result = new List<int>();

        foreach (var skillId in candidateSkillIds)
        {
            if (!allowed.Contains(skillId) || !seen.Add(skillId))
            {
                continue;
            }

            result.Add(skillId);
        }

        return result;
    }
}
