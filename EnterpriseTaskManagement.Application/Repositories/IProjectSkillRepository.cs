using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IProjectSkillRepository
    {
        Task<ProjectSkill?> GetAsync(int projectId, int skillId);

        Task<IEnumerable<ProjectSkill>> GetByProjectIdAsync(int projectId);

        Task AddAsync(ProjectSkill projectSkill);

        Task DeleteAsync(ProjectSkill projectSkill);
    }
}