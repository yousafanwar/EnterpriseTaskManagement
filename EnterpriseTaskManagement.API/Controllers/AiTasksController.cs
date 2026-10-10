using EnterpriseTaskManagement.API.Contracts.AI;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class AiTasksController : ControllerBase
{
    private readonly IAiTaskWorkflowService _workflowService;

    public AiTasksController(IAiTaskWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpPost("analyze")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> AnalyzeAndCreateTask(
        [FromForm] AnalyzeTaskFormRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.FeedbackText) && request.Image is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid argument.",
                detail: "FeedbackText or an image is required.");
        }

        var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        ImageUploadRequest? image = null;
        if (request.Image is not null)
        {
            image = new ImageUploadRequest
            {
                Content = request.Image.OpenReadStream(),
                FileName = request.Image.FileName,
                ContentType = request.Image.ContentType,
                Length = request.Image.Length
            };
        }

        var command = new AnalyzeTaskCommand
        {
            ProjectId = request.ProjectId,
            FeedbackText = request.FeedbackText,
            Image = image,
            ActorUserId = actorUserId,
            IsAdmin = isAdmin
        };

        var task = await _workflowService.AnalyzeAndCreateTaskAsync(
            command,
            cancellationToken);

        return CreatedAtAction(
            "GetTask",
            "Tasks",
            new { id = task.Id },
            task);
    }
}
