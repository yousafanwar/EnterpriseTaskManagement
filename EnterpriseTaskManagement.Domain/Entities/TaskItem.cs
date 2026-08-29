using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Domain.Enums;

namespace EnterpriseTaskManagement.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public TaskItemStatus Status { get; private set; }
        public int? AssigneeId { get; private set; }
        public User? Assignee { get; private set; }

        private TaskItem () 
        { 
        }

        public TaskItem (string title, string description)
        {
            Title = title;
            Description = description;
            Status = TaskItemStatus.Pending;
        }

        public void AssignTo(int userId)
        {
            AssigneeId = userId;
        }

        public void Complete()
        {
            if(AssigneeId == null)
            {
                throw new InvalidOperationException
                    (
                        "A task must have a assignee before it can be marked complete"
                    );
            }
            Status = TaskItemStatus.Completed;
        }

        public void UpdateDetails(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
