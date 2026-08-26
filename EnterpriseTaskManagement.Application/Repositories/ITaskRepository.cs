using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface ITaskRepository
    {
        string GetTaskTitle();

        TaskItem? GetById(int Id);
        void Add(TaskItem task);
        IEnumerable<TaskItem> GetAll();
        void Update(TaskItem task);
        void Delete(TaskItem task);
    }
}
