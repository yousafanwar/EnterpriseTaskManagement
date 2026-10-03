namespace EnterpriseTaskManagement.Domain.Entities
{
    public class Skill
    {
        public int Id { get; private set; }

        public string Name { get; private set; } = null!;

        public ICollection<UserSkill> UserSkills { get; private set; } = new List<UserSkill>();

        public ICollection<ProjectSkill> ProjectSkills { get; private set; } = new List<ProjectSkill>();

        private Skill()
        {
        }

        public Skill(string name)
        {
            Name = name;
        }
    }
}