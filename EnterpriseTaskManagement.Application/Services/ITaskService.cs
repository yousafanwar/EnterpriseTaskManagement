using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services
{
    public interface ITaskService
    {
        Task<TaskItemDto?> GetTaskAsync(int id);
        Task<TaskItemDto> AddTaskAsync(CreateTaskRequest request);
        Task<IEnumerable<TaskItemDto>> GetTasksAsync();
        Task<TaskItemDto?> UpdateTaskAsync(int id, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);
        Task<TaskItemDto?> AssignTaskAsync(int taskId, int userId);
        Task<TaskItemDto?> StartTaskAsync(int id);
        Task<TaskItemDto?> CompleteTaskAsync(int id);
    }
}
