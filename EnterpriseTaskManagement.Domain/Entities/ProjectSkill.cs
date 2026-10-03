namespace EnterpriseTaskManagement.Domain.Entities
{
    public class ProjectSkill
    {
        public int ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;

        public int SkillId { get; private set; }
        public Skill Skill { get; private set; } = null!;

        private ProjectSkill()
        {
        }

        public ProjectSkill(int projectId, int skillId)
        {
            ProjectId = projectId;
            SkillId = skillId;
        }
    }
}