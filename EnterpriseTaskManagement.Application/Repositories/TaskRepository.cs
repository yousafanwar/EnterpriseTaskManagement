using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();

        public TaskRepository()
        {
            var task = new TaskItem(
                1,
                "Copy Ninja Hatake No Kakashi",
                "I am honored to meet you! now where is the nine tails demon fox"
                );

            task.AssignTo(1);
            _tasks.Add(task);

        }
        public string GetTaskTitle()
        {
            return "Fix authentication bug";
        }

        public TaskItem GetById(int id)
        {
            return _tasks.FirstOrDefault(task => task.Id == id);
        }

        public void Add(TaskItem task)
        {
            _tasks.Add(task);
        }
    }
}
