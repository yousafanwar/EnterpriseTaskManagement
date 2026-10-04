using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface ITaskRepository
    {
        Task <TaskItem?> GetByIdAsync(int Id);
        Task AddTask(TaskItem task);
        Task <(IEnumerable<TaskItem> Tasks, int TotalCount)> GetAllAsync(TaskQueryRequest request);
        Task UpdateAsync(TaskItem task);
        Task ReloadAssigneeAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}
