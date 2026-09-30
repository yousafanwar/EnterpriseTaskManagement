using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class ProjectSkillRepository : IProjectSkillRepository
    {
        private readonly TaskDbContext _context;

        public ProjectSkillRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectSkill?> GetAsync(
            int projectId,
            int skillId)
        {
            return await _context.ProjectSkills
                .FirstOrDefaultAsync(
                    ps => ps.ProjectId == projectId
                    && ps.SkillId == skillId);
        }

        public async Task<IEnumerable<ProjectSkill>> GetByProjectIdAsync(
            int projectId)
        {
            return await _context.ProjectSkills
                .Where(ps => ps.ProjectId == projectId)
                .Include(ps => ps.Skill)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(ProjectSkill projectSkill)
        {
            await _context.ProjectSkills.AddAsync(projectSkill);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProjectSkill projectSkill)
        {
            _context.ProjectSkills.Remove(projectSkill);
            await _context.SaveChangesAsync();
        }
    }
}