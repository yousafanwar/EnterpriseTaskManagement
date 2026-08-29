using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
