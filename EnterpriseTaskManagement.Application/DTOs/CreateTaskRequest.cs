using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a valid existing project id.")]
        public int ProjectId { get; set; }
    }
}
