using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnterpriseTaskManagement.Application.DTOs.User
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        public int RoleId { get; set; }
    }
}
