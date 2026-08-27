using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface ITaskRepository
    {
        Task <string> GetTaskTitleAsync();

        Task <TaskItem?> GetByIdAsync(int Id);
        Task AddTask(TaskItem task);
        Task <IEnumerable<TaskItem>> GetAllAsync();
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}
