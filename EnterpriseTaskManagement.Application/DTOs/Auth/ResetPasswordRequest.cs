using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
