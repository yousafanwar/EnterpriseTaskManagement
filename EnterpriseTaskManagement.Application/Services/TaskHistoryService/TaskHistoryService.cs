using EnterpriseTaskManagement.Application.DTOs.TaskHistory;
using EnterpriseTaskManagement.Application.Interfaces.Repositories;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ITaskHistoryRepository _taskHistoryRepository;

        public TaskHistoryService(ITaskHistoryRepository taskHistoryRepository)
        {
            _taskHistoryRepository = taskHistoryRepository;
        }

        public async Task AddHistoryAsync(
            int taskId,
            int userId,
            string action,
            string? oldValue = null,
            string? newValue = null)
        {
            var history = new TaskHistory(
                taskId,
                userId,
                action,
                oldValue,
                newValue);

            await _taskHistoryRepository.AddHistoryAsync(history);
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(int taskId)
        {
            var history = await _taskHistoryRepository.GetByTaskIdAsync(taskId);

            return history.Select(h => new TaskHistoryDto
            {
                Id = h.Id,
                TaskId = h.TaskId,
                UserId = h.UserId,
                UserName = $"{h.User.FirstName} {h.User.LastName}",
                Action = h.Action,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                CreatedAt = h.CreatedAt
            });
        }
    }
}