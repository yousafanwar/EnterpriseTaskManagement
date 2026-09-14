namespace EnterpriseTaskManagement.Domain.Entities
{
    public class TaskHistory
    {
        public int Id { get; private set; }

        public int TaskId { get; private set; }
        public TaskItem Task { get; private set; } = null!;

        public int? UserId { get; private set; }
        public User? User { get; private set; } = null!;

        public string Action { get; private set; } = string.Empty;

        public string? OldValue { get; private set; }

        public string? NewValue { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private TaskHistory()
        {
        }

        public TaskHistory(
            int taskId,
            int? userId,
            string action,
            string? oldValue,
            string? newValue)
        {
            TaskId = taskId;
            UserId = userId;
            Action = action;
            OldValue = oldValue;
            NewValue = newValue;
            CreatedAt = DateTime.UtcNow;
        }
    }
}