namespace EnterpriseTaskManagement.Application.Services.AuthService
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}