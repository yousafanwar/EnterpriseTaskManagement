using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly IProjectSkillRepository _projectSkillRepository;
        private readonly IProjectUserRepository _projectUserRepository;
        private readonly IUserSkillRepository _userSkillRepository;

        public ProjectService(
            IProjectRepository repository,
            IProjectSkillRepository projectSkillRepository,
            IProjectUserRepository projectUserRepository,
            IUserSkillRepository userSkillRepository)
        {
            _repository = repository;
            _projectSkillRepository = projectSkillRepository;
            _projectUserRepository = projectUserRepository;
            _userSkillRepository = userSkillRepository;
        }

        public async Task<ProjectDto?> GetProjectAsync(int id)
        {
            var project = await _repository.GetByIdAsync(id);

            if (project == null)
                return null;

            return MapToDto(project);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync()
        {
            var projects = await _repository.GetAllAsync();

            return projects.Select(MapToDto);
        }

        public async Task<ProjectDto> AddProjectAsync(
            CreateProjectRequest request)
        {
            var project = new Project(
                request.Name,
                request.Description,
                request.StartDate,
                request.EndDate);

            await _repository.AddAsync(project);

            return MapToDto(project);
        }

        public async Task<ProjectDto?> UpdateProjectAsync(
            int id,
            UpdateProjectRequest request)
        {
            var project = await _repository.GetByIdAsync(id);

            if (project == null)
                return null;

            project.UpdateDetails(
                request.Name,
                request.Description,
                request.StartDate,
                request.EndDate);

            await _repository.UpdateAsync(project);

            return MapToDto(project);
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _repository.GetByIdAsync(id);

            if (project == null)
                return false;

            await _repository.DeleteAsync(project);

            return true;
        }


        // Project Skills

        public async Task<IEnumerable<ProjectSkillDto>> GetProjectSkillsAsync(
            int projectId)
        {
            var projectSkills =
                await _projectSkillRepository.GetByProjectIdAsync(projectId);

            return projectSkills.Select(ps => new ProjectSkillDto
            {
                SkillId = ps.SkillId,
                SkillName = ps.Skill.Name
            });
        }

        public async Task<bool> AddProjectSkillAsync(
            int projectId,
            int skillId)
        {
            var existing =
                await _projectSkillRepository.GetAsync(projectId, skillId);

            if (existing != null)
                return false;

            var projectSkill = new ProjectSkill(
                projectId,
                skillId);

            await _projectSkillRepository.AddAsync(projectSkill);

            return true;
        }

        public async Task<bool> RemoveProjectSkillAsync(
            int projectId,
            int skillId)
        {
            var projectSkill =
                await _projectSkillRepository.GetAsync(
                    projectId,
                    skillId);

            if (projectSkill == null)
                return false;

            await _projectSkillRepository.DeleteAsync(projectSkill);

            return true;
        }


        // Project Users

        public async Task<IEnumerable<ProjectUserDto>> GetProjectUsersAsync(
            int projectId)
        {
            var projectUsers =
                await _projectUserRepository.GetByProjectIdAsync(projectId);

            return projectUsers.Select(pu => new ProjectUserDto
            {
                UserId = pu.UserId,
                FirstName = pu.User.FirstName,
                LastName = pu.User.LastName,
                Email = pu.User.Email
            });
        }

        public async Task<bool> AddProjectUserAsync(
            int projectId,
            int userId)
        {
            var existing =
                await _projectUserRepository.GetAsync(
                    projectId,
                    userId);

            if (existing != null)
                return false;

            // Get project required skills
            var projectSkills =
                await _projectSkillRepository.GetByProjectIdAsync(
                    projectId);

            // Get user's skills
            var userSkills =
                await _userSkillRepository.GetByUserIdAsync(
                    userId);

            // ANY rule:
            // User needs at least one skill required by the project.
            var hasRequiredSkill = projectSkills.Any(projectSkill =>
                userSkills.Any(userSkill =>
                    userSkill.SkillId == projectSkill.SkillId));

            if (!hasRequiredSkill)
            {
                throw new InvalidOperationException(
                    "User does not have any required skill for this project.");
            }

            var projectUser = new ProjectUser(
                projectId,
                userId);

            await _projectUserRepository.AddAsync(projectUser);

            return true;
        }

        public async Task<bool> RemoveProjectUserAsync(
            int projectId,
            int userId)
        {
            var projectUser =
                await _projectUserRepository.GetAsync(
                    projectId,
                    userId);

            if (projectUser == null)
                return false;

            await _projectUserRepository.DeleteAsync(projectUser);

            return true;
        }


        // Mapping

        private static ProjectDto MapToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };
        }
    }
}