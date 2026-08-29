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
        private readonly IUserRepository _userRepository;

        public TaskService (
            ITaskRepository repository,
            IUserRepository userRepository
            )
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<TaskItemDto?> GetTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if(task == null)
            {
                return null;
            }

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
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
                Description = task.Description,
                Status = task.Status
            };
        }

        public async Task<PagedResult<TaskItemDto>> GetTasksAsync(
            TaskQueryRequest request)
        {
            var result = await _repository.GetAllAsync(request);

            var tasks = result.Tasks.Select(task => new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            });

            return new PagedResult<TaskItemDto>
            {
                Items = tasks,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling(
                    result.TotalCount / (double)request.PageSize)
            };
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
                Description = task.Description,
                Status = task.Status
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

        public async Task<TaskItemDto?> AssignTaskAsync(int taskId, int userId)
        {
            var task = await _repository.GetByIdAsync(taskId);

            if(task == null)
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            
            if(user == null)
            {
                return null;
            }

            task.AssignTo(userId);

            await _repository.UpdateAsync(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            };                
        }

        public async Task<TaskItemDto?> StartTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if(task == null)
            {
                return null;
            }

            task.Start();

            await _repository.UpdateAsync(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            };
        }

        public async Task<TaskItemDto?> CompleteTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return null;
            }

            task.Complete();

            await _repository.UpdateAsync(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            };

        }
    }
}
