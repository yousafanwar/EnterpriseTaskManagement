namespace EnterpriseTaskManagement.Domain.Entities
{
    public class ProjectUser
    {
        public int ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;

        public int UserId { get; private set; }
        public User User { get; private set; } = null!;

        private ProjectUser()
        {
        }

        public ProjectUser(int projectId, int userId)
        {
            ProjectId = projectId;
            UserId = userId;
        }
    }
}