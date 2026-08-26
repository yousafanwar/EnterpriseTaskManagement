using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Application.Repositories;

namespace EnterpriseTaskManagement.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext;

    }
    public string GetTaskTitle()
    {
        return _dbContext.Tasks.Select(task => task.Title).FirstOrDefault() ?? "No tasks found";
    }

    public TaskItem? GetById(int id)
    {
        return _dbContext.Tasks.FirstOrDefault(task => task.Id == id);
    }

    public void Add(TaskItem task)
    {
        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();
    }

    public IEnumerable<TaskItem> GetAll()
    {
        return _dbContext.Tasks.ToList();
    }

    public void Update(TaskItem task)
    {
        _dbContext.Tasks.Update(task);
        _dbContext.SaveChanges();

    }
    public void Delete(TaskItem task)
    {
        _dbContext.Tasks.Remove(task);
        _dbContext.SaveChanges();
    }
}
