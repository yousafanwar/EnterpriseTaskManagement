using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.User
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
    }
}
