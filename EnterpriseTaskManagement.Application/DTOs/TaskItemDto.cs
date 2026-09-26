using EnterpriseTaskManagement.Domain.Enums;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public AssigneeDto? Assignee { get; set; }
    }
}
