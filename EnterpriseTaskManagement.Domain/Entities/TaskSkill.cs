namespace EnterpriseTaskManagement.Domain.Entities;

public class TaskSkill
{
    public int TaskId { get; private set; }
    public TaskItem Task { get; private set; } = null!;

    public int SkillId { get; private set; }
    public Skill Skill { get; private set; } = null!;

    private TaskSkill()
    {
    }

    public TaskSkill(int taskId, int skillId)
    {
        TaskId = taskId;
        SkillId = skillId;
    }
}
