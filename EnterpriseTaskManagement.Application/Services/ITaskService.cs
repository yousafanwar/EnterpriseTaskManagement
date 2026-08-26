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
        string GetMessage();
        TaskItemDto? GetTask(int Id);
        TaskItemDto AddTask(CreateTaskRequest request);
        IEnumerable<TaskItemDto> GetTasks();
        TaskItemDto? UpdateTask(int Id, UpdateTaskRequest request);
        bool DeleteTask(int id);
    }
}
