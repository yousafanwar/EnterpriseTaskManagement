using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class ProjectUserRepository : IProjectUserRepository
    {
        private readonly TaskDbContext _context;

        public ProjectUserRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectUser?> GetAsync(
            int projectId,
            int userId)
        {
            return await _context.ProjectUsers
                .FirstOrDefaultAsync(
                    pu => pu.ProjectId == projectId
                    && pu.UserId == userId);
        }

        public async Task<IEnumerable<ProjectUser>> GetByProjectIdAsync(
            int projectId)
        {
            return await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Include(pu => pu.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(ProjectUser projectUser)
        {
            await _context.ProjectUsers.AddAsync(projectUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProjectUser projectUser)
        {
            _context.ProjectUsers.Remove(projectUser);
            await _context.SaveChangesAsync();
        }
    }
}