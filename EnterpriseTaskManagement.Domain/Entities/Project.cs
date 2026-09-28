namespace EnterpriseTaskManagement.Domain.Entities
{
    public class Project
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public ICollection<ProjectSkill> ProjectSkills { get; private set; } = new List<ProjectSkill>();

        public ICollection<ProjectUser> ProjectUsers { get; private set; } = new List<ProjectUser>();
        public ICollection<TaskItem> Tasks { get; private set; } = new List<TaskItem>();

        private Project()
        {
        }

        public Project(
            string name,
            string description,
            DateTime startDate,
            DateTime? endDate)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}