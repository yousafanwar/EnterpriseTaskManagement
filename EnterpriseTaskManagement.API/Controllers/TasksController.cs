using EnterpriseTaskManagement.Application.Services;
using EnterpriseTaskManagement.Domain.Entities;
using EnterpriseTaskManagement.Application.DTOs;
using System.Security.Claims;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskAsync(id);
        if(task == null)
        {
            return NotFound();
        }

        return task;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskRequest request)  
    {
        var task = await _taskService.AddTaskAsync(request);
        //return Ok(task);
        return CreatedAtAction(
                nameof(GetTask),
                new { id = task.Id},
                task
            );
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskItemDto>>> GetTasks(
        [FromQuery] TaskQueryRequest request)
    {
        var result = await _taskService.GetTasksAsync(request);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(int id, UpdateTaskRequest request)
    {
        var task = await _taskService.UpdateTaskAsync(id, request);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await _taskService.DeleteTaskAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
    [HttpPut("{taskId}/assign/{userId}")]
    public async Task<ActionResult<TaskItemDto?>> AssignTask(int taskId, int userId)
    {
        var actorUserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var task = await _taskService.AssignTaskAsync(taskId, userId, actorUserId);

        if(task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPut("{id}/start")]
    public async Task<ActionResult<TaskItemDto?>> StartTask(int id)
    {
        var actorUserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var task = await _taskService.StartTaskAsync(id, actorUserId);
        
        if (task == null)
        {
            return NotFound();
        }
        
        return Ok(task);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<TaskItemDto?>> CompleteTask(int id)
    {
        var actorUserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var task = await _taskService.CompleteTaskAsync(id, actorUserId);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

}
