using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ProjectDto?> GetProjectAsync(int id);
        Task<IEnumerable<ProjectDto>> GetProjectsAsync();
        Task<ProjectDto> AddProjectAsync(CreateProjectRequest request);
        Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request);
        Task<bool> DeleteProjectAsync(int id);

        //ProjectSkills
        Task<IEnumerable<ProjectSkillDto>> GetProjectSkillsAsync(int projectId);
        Task<bool> AddProjectSkillAsync(int projectId, int skillId);
        Task<bool> RemoveProjectSkillAsync(int projectId, int skillId);

        //ProjectUsers
        Task<IEnumerable<ProjectUserDto>> GetProjectUsersAsync(int projectId);
        Task<bool> AddProjectUserAsync(int projectId, int userId);
        Task<bool> RemoveProjectUserAsync(int projectId, int userId);
    }
}