using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;

        public TaskService (ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<TaskItemDto?> GetTaskAsync(int Id)
        {
            var task = await _repository.GetByIdAsync(Id);

            if(task == null)
            {
                return null;
            }

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };
        }

        public async Task <TaskItemDto> AddTaskAsync(CreateTaskRequest request)
        {
            var task = new TaskItem(
                    request.Title,
                    request.Description
                );            
            
            await _repository.AddTask(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksAsync()
        {
            var tasks = await _repository.GetAllAsync();
            return tasks.Select(task => new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            });
        }

        public async Task <TaskItemDto?> UpdateTaskAsync(int id, UpdateTaskRequest request)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return null;
            }

            task.UpdateDetails(
                request.Title,
                request.Description
            );

            await _repository.UpdateAsync(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };
        }
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return false;
            }

            await _repository.DeleteAsync(task);

            return true;
        }
    }
}
