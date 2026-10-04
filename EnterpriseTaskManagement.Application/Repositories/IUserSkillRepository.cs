using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IUserSkillRepository
    {
        Task<UserSkill?> GetAsync(int userId, int skillId);
        Task<IEnumerable<UserSkill>> GetByUserIdAsync(int userId);
        Task AddAsync(UserSkill userSkill);
        Task DeleteAsync(UserSkill userSkill);
    }
}