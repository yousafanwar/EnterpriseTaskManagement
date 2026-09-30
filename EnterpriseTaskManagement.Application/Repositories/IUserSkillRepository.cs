using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IUserSkillRepository
    {
        Task<IEnumerable<UserSkill>> GetByUserIdAsync(int userId);
    }
}