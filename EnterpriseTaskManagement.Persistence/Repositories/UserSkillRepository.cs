using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class UserSkillRepository : IUserSkillRepository
    {
        private readonly TaskDbContext _context;

        public UserSkillRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserSkill>> GetByUserIdAsync(int userId)
        {
            return await _context.UserSkills
                .Where(us => us.UserId == userId)
                .Include(us => us.Skill)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}