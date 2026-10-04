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
        return await _dbContext.Tasks
            .Include(task => task.Assignee)
            .Include(task => task.Project)
            .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task AddTask(TaskItem task)
    {
        await _dbContext.Tasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(IEnumerable<TaskItem> Tasks, int TotalCount)> GetAllAsync(TaskQueryRequest request)
    {
        //return await _dbContext.Tasks.ToListAsync();

        var query = _dbContext.Tasks
            .Include(task => task.Assignee)
            .Include(task => task.Project)
            .AsQueryable();

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

        switch (request.SortBy?.ToLower())
        {
            case "title":
                query = request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(task => task.Title)
                    : query.OrderBy(task => task.Title);
                break;

            case "status":
                query = request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(task => task.Status)
                    : query.OrderBy(task => task.Status);
                break;
            case "description":
                query = request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(task => task.Description)
                    : query.OrderBy(task => task.Description);
                break;

            default:
                query = request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(task => task.Id)
                    : query.OrderBy(task => task.Id);
                break;
        }

        var totalCount = await query.CountAsync();

        var tasks = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    public async Task UpdateAsync(TaskItem task)
    {
        // Entity is already tracked from GetByIdAsync. Avoid DbSet.Update(),
        // which marks the whole graph Modified and can null out AssigneeId
        // when the Assignee navigation is still null/stale after AssignTo.
        await _dbContext.SaveChangesAsync();
    }

    public async Task ReloadAssigneeAsync(TaskItem task)
    {
        await _dbContext.Entry(task).Reference(t => t.Assignee).LoadAsync();
    }
    public async Task DeleteAsync(TaskItem task)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();
    }
}
