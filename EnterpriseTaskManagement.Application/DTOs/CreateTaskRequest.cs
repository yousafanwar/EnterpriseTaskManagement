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
        public string Title { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
    }
}
