using EnterpriseTaskManagement.Application.DTOs.TaskHistory;

namespace EnterpriseTaskManagement.Application.Interfaces.Services
{
    public interface ITaskHistoryService
    {
        Task AddHistoryAsync(
            int taskId,
            int userId,
            string action,
            string? oldValue = null,
            string? newValue = null);

        Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(int taskId);
    }
}