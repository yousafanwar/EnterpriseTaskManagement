using EnterpriseTaskManagement.Application.Services;
using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("message")]
    public IActionResult GetMessage()
    {
        var message = _taskService.GetMessage();
        return Ok(message);
    }

    [HttpGet("{id}")]
    public ActionResult<TaskItemDto> GetTask(int id)
    {
        var task = _taskService.GetTask(id);
        if(task == null)
        {
            return NotFound();
        }

        return task;
    }

    [HttpPost]
    public ActionResult<TaskItemDto> CreateTask(CreateTaskRequest request)  
    {
        var task = _taskService.AddTask(request);
        //return Ok(task);
        return CreatedAtAction(
                nameof(GetTask),
                new { id = task.Id},
                task
            );
    }

    [HttpGet]
    public ActionResult<IEnumerable<TaskItemDto>> GetTasks()
    {
        var tasks = _taskService.GetTasks();
        return Ok(tasks);
    }

    [HttpPut("{id}")]
    public ActionResult<TaskItemDto> UpdateTask(int id, UpdateTaskRequest request)
    {
        var task = _taskService.UpdateTask(id, request);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        var deleted = _taskService.DeleteTask(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
