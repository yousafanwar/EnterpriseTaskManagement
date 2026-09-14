using EnterpriseTaskManagement.Application.DTOs.TaskHistory;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/history")]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;

        public TaskHistoryController(ITaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetTaskHistory(int taskId)
        {
            var history = await _taskHistoryService.GetTaskHistoryAsync(taskId);

            return Ok(history);
        }
    }
}