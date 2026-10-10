using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.Auth
{
    public class AdminResetPasswordRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
