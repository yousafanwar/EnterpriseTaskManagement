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

        public TaskItem? GetTask(int Id)
        {
            return _repository.GetById(Id);
        }

        public void AddTask(TaskItem task)
        {
            _repository.Add(task);
        }
    }
}
