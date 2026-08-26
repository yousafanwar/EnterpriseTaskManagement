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
        public string GetMessage()
        {
            return _repository.GetTaskTitle();
        }

        public TaskItemDto? GetTask(int Id)
        {
            var task = _repository.GetById(Id);

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

        public TaskItemDto AddTask(CreateTaskRequest request)
        {
            var task = new TaskItem(
                    request.Title,
                    request.Description
                );            
            
            _repository.Add(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };
        }

        public IEnumerable<TaskItemDto> GetTasks()
        {
            var tasks = _repository.GetAll();
            return tasks.Select(task => new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            });
        }

        public TaskItemDto? UpdateTask(int id, UpdateTaskRequest request)
        {
            var task = _repository.GetById(id);

            if (task == null)
            {
                return null;
            }

            task.UpdateDetails(
                request.Title,
                request.Description
            );

            _repository.Update(task);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };
        }
        public bool DeleteTask(int id)
        {
            var task = _repository.GetById(id);

            if (task == null)
            {
                return false;
            }

            _repository.Delete(task);

            return true;
        }
    }
}
