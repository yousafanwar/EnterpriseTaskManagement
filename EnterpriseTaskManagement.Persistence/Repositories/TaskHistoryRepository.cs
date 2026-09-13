using EnterpriseTaskManagement.Application.Interfaces.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class TaskHistoryRepository : ITaskHistoryRepository
    {
        private readonly TaskDbContext _context;

        public TaskHistoryRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(TaskHistory history)
        {
            await _context.TaskHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskHistory>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskHistories
                .Where(h => h.TaskId == taskId)
                .Include(h => h.User)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}