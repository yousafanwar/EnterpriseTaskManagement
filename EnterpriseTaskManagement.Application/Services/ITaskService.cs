using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services
{
    public interface ITaskService
    {
        string GetMessage();
        TaskItem? GetTask(int Id);
        void AddTask(TaskItem task);
    }
}
