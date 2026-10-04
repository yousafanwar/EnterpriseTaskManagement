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

        public async Task<UserSkill?> GetAsync(int userId, int skillId)
        {
            return await _context.UserSkills
                .FirstOrDefaultAsync(us =>
                    us.UserId == userId && us.SkillId == skillId);
        }

        public async Task<IEnumerable<UserSkill>> GetByUserIdAsync(int userId)
        {
            return await _context.UserSkills
                .Where(us => us.UserId == userId)
                .Include(us => us.Skill)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(UserSkill userSkill)
        {
            await _context.UserSkills.AddAsync(userSkill);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserSkill userSkill)
        {
            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();
        }
    }
}
