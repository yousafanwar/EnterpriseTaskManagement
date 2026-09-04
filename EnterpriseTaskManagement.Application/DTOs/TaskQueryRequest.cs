using EnterpriseTaskManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class TaskQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
        public TaskItemStatus? Status { get; set; }
        public int? AssigneeId { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
