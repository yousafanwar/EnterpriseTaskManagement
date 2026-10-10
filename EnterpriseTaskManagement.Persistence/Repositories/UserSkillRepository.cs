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

        public async Task<IReadOnlyDictionary<int, HashSet<int>>> GetSkillIdsByUserIdsAsync(
            IEnumerable<int> userIds,
            CancellationToken cancellationToken = default)
        {
            var idList = userIds.Distinct().ToList();
            if (idList.Count == 0)
            {
                return new Dictionary<int, HashSet<int>>();
            }

            var rows = await _context.UserSkills
                .Where(us => idList.Contains(us.UserId))
                .AsNoTracking()
                .Select(us => new { us.UserId, us.SkillId })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.SkillId).ToHashSet());
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
