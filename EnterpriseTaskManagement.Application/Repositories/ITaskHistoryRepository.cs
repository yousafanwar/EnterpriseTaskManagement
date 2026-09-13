using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Interfaces.Repositories
{
    public interface ITaskHistoryRepository
    {
        Task AddHistoryAsync(TaskHistory history);

        Task<IEnumerable<TaskHistory>> GetByTaskIdAsync(int taskId);
    }
}