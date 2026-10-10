using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly ILogger<TaskService> _logger;

        public TaskService (
            ITaskRepository repository,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            ITaskHistoryService taskHistoryService,
            ILogger<TaskService> logger
            )
        {
            _repository = repository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _taskHistoryService = taskHistoryService;
            _logger = logger;
        }

        public async Task<TaskItemDto?> GetTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if(task == null)
            {
                return null;
            }

            return MapToDto(task);
        }

        private static TaskItemDto MapToDto(TaskItem task)
        {
            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? string.Empty,
                Assignee = task.Assignee == null
                    ? null
                    : new AssigneeDto
                    {
                        Id = task.Assignee.Id,
                        FirstName = task.Assignee.FirstName,
                        LastName = task.Assignee.LastName,
                        Email = task.Assignee.Email
                    },
                OriginalFeedback = task.OriginalFeedback,
                FeedbackImageUrl = task.FeedbackImageUrl,
                RequiredSkillIds = task.RequiredSkills
                    .Select(ts => ts.SkillId)
                    .ToList()
            };
        }

        public async Task <TaskItemDto> AddTaskAsync(CreateTaskRequest request)
        {
            if (request.ProjectId <= 0)
            {
                throw new ArgumentException(
                    "ProjectId is required and must be a valid project id. Create a project first, then pass its id.",
                    nameof(request.ProjectId)
                );
            }

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);

            if (project == null)
            {
                throw new KeyNotFoundException(
                    $"Project with id {request.ProjectId} was not found."
                );
            }

            var task = new TaskItem(
                    request.Title,
                    request.Description,
                    request.ProjectId
                );            
            
            await _repository.AddTask(task);

            await _taskHistoryService.AddHistoryAsync(
                task.Id,
                null,
                "Created");

            _logger.LogInformation("Task {TaskId} created successfully", task.Id);

            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                ProjectId = task.ProjectId,
                ProjectName = project.Name,
                Assignee = null
            };
        }

        public async Task<PagedResult<TaskItemDto>> GetTasksAsync(
            TaskQueryRequest request)
        {
            var result = await _repository.GetAllAsync(request);

            var tasks = result.Tasks.Select(MapToDto);

            return new PagedResult<TaskItemDto>
            {
                Items = tasks,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling(
                    result.TotalCount / (double)request.PageSize)
            };
        }

        public async Task <TaskItemDto?> UpdateTaskAsync(int id, UpdateTaskRequest request)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return null;
            }

            task.UpdateDetails(
                request.Title,
                request.Description
            );

            await _repository.UpdateAsync(task);

            return MapToDto(task);
        }
        public async Task<bool> DeleteTaskAsync(int id, int userId)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return false;
            }

            await _repository.DeleteAsync(task);

            _logger.LogInformation("Task {TaskId} deleted by user {ActorUserId}.", task.Id, userId);

            return true;
        }

        public async Task<TaskItemDto?> AssignTaskAsync(int taskId, int userId, int actorUserId)
        {
            var task = await _repository.GetByIdAsync(taskId);

            if(task == null)
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            
            if(user == null)
            {
                return null;
            }

            var oldUserId = task.AssigneeId;

            task.AssignTo(userId);

            await _repository.UpdateAsync(task);
            await _repository.ReloadAssigneeAsync(task);

            await _taskHistoryService.AddHistoryAsync(
                task.Id,
                actorUserId,
                "Assigned",
                oldUserId?.ToString(),
                userId.ToString()
            );

            _logger.LogInformation("Task {TaskId} has been assigned to {AssignedId} by {AssigneeId}", task.Id, userId, actorUserId);

            var dto = MapToDto(task);
            // Ensure response includes assignee even if navigation reload is skipped.
            dto.Assignee ??= new AssigneeDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
            return dto;
        }

        public async Task<TaskItemDto?> StartTaskAsync(int id, int actorUserId)
        {
            var task = await _repository.GetByIdAsync(id);

            if(task == null)
            {
                return null;
            }

            var oldStatus = task.Status;

            task.Start();

            await _repository.UpdateAsync(task);

            await _taskHistoryService.AddHistoryAsync(
                task.Id,
                actorUserId,
                "StatusChanged",
                oldStatus.ToString(),
                task.Status.ToString());

            _logger.LogInformation(
                "Task {TaskId} started by user {ActorUserId}.",
                task.Id,
                actorUserId
            );

            return MapToDto(task);
        }

        public async Task<TaskItemDto?> CompleteTaskAsync(int id, int actorUserId)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return null;
            }

            var oldStatus = task.Status;

            task.Complete();

            await _repository.UpdateAsync(task);

            await _taskHistoryService.AddHistoryAsync(
                task.Id,
                actorUserId,
                "StatusChanged",
                oldStatus.ToString(),
                task.Status.ToString());

            _logger.LogInformation(
                "Task {TaskId} completed by user {ActorUserId}.",
                task.Id,
                actorUserId
            );

            return MapToDto(task);

        }
    }
}
