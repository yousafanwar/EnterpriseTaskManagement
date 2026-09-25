using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}