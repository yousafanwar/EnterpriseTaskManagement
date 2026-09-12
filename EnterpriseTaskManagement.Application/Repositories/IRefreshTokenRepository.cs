using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task UpdateAsync(RefreshToken refreshToken);
    }
}