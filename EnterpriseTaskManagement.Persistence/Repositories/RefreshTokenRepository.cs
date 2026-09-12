using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly TaskDbContext _context;

        public RefreshTokenRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.Revoked);
        }
        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}