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
    public IActionResult GetTask(int id)
    {
        var task = _taskService.GetTask(id);
        if(task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public IActionResult CreateTask(CreateTaskRequest request)
    {
        var task = new TaskItem(
                request.Title,
                request.Description
            );
        _taskService.AddTask(task);
        return Ok(task);
    }
}
