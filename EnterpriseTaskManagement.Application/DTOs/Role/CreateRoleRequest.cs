using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnterpriseTaskManagement.Application.DTOs.Role
{
    public class CreateRoleRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
