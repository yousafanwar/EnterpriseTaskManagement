using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.User
{
    public class UpdateUserRequest
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
