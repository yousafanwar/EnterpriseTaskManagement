using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Domain.Entities
{
    public class Comment
    {
        public int Id { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int TaskId { get; private set; }
        public TaskItem Task { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; }

        // EF Core needs a parameterless constructor it can use when materializing entities from the database.
        private Comment()
        {
        }

        public Comment(string content, int taskId, int userId)
        {
            Content = content;
            TaskId = taskId;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

    }
}
