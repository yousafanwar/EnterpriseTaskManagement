using Azure;
using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext;

    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _dbContext.Tasks.FindAsync(id);
    }

    public async Task AddTask(TaskItem task)
    {
        await _dbContext.Tasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(IEnumerable<TaskItem> Tasks, int TotalCount)> GetAllAsync(TaskQueryRequest request)
    {
        //return await _dbContext.Tasks.ToListAsync();

        var query = _dbContext.Tasks.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(task =>
                task.Title.Contains(request.Search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(task => task.Status == request.Status);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(task => task.AssigneeId == request.AssigneeId);
        }

        var totalCount = await query.CountAsync();

        var tasks = await query
            .OrderBy(task => task.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync();

    }
    public async Task DeleteAsync(TaskItem task)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();
    }
}
