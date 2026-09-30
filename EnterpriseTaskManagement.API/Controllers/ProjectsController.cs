using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _projectService.GetProjectsAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _projectService.GetProjectAsync(id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject(
            CreateProjectRequest request)
        {
            var project = await _projectService.AddProjectAsync(request);

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                project);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(
            int id,
            UpdateProjectRequest request)
        {
            var project =
                await _projectService.UpdateProjectAsync(id, request);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var deleted =
                await _projectService.DeleteProjectAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }


        // Project Skills

        [HttpGet("{projectId}/skills")]
        public async Task<ActionResult<IEnumerable<ProjectSkillDto>>>
            GetProjectSkills(int projectId)
        {
            var skills =
                await _projectService.GetProjectSkillsAsync(projectId);

            return Ok(skills);
        }

        [HttpPost("{projectId}/skills/{skillId}")]
        public async Task<IActionResult> AddProjectSkill(
            int projectId,
            int skillId)
        {
            var added =
                await _projectService.AddProjectSkillAsync(
                    projectId,
                    skillId);

            if (!added)
                return Conflict("Skill is already assigned to this project.");

            return NoContent();
        }

        [HttpDelete("{projectId}/skills/{skillId}")]
        public async Task<IActionResult> RemoveProjectSkill(
            int projectId,
            int skillId)
        {
            var removed =
                await _projectService.RemoveProjectSkillAsync(
                    projectId,
                    skillId);

            if (!removed)
                return NotFound();

            return NoContent();
        }


        // Project Users


        [HttpGet("{projectId}/users")]
        public async Task<ActionResult<IEnumerable<ProjectUserDto>>>
            GetProjectUsers(int projectId)
        {
            var users =
                await _projectService.GetProjectUsersAsync(projectId);

            return Ok(users);
        }

        [HttpPost("{projectId}/users/{userId}")]
        public async Task<IActionResult> AddProjectUser(
            int projectId,
            int userId)
        {
            var added =
                await _projectService.AddProjectUserAsync(
                    projectId,
                    userId);

            if (!added)
                return Conflict("User is already assigned to this project.");

            return NoContent();
        }

        [HttpDelete("{projectId}/users/{userId}")]
        public async Task<IActionResult> RemoveProjectUser(
            int projectId,
            int userId)
        {
            var removed =
                await _projectService.RemoveProjectUserAsync(
                    projectId,
                    userId);

            if (!removed)
                return NotFound();

            return NoContent();
        }
    }
}