using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(int id);

        Task<IEnumerable<Project>> GetAllAsync();

        Task AddAsync(Project project);

        Task UpdateAsync(Project project);

        Task DeleteAsync(Project project);
    }
}