using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IProjectUserRepository
    {
        Task<ProjectUser?> GetAsync(int projectId, int userId);

        Task<IEnumerable<ProjectUser>> GetByProjectIdAsync(int projectId);

        Task AddAsync(ProjectUser projectUser);

        Task DeleteAsync(ProjectUser projectUser);
    }
}