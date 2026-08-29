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
        Task<TaskItemDto?> GetTaskAsync(int Id);
        Task<TaskItemDto> AddTaskAsync(CreateTaskRequest request);
        Task<IEnumerable<TaskItemDto>> GetTasksAsync();
        Task<TaskItemDto?> UpdateTaskAsync(int Id, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);
    }
}
