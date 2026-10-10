using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IUserSkillRepository
    {
        Task<UserSkill?> GetAsync(int userId, int skillId);
        Task<IEnumerable<UserSkill>> GetByUserIdAsync(int userId);
        Task<IReadOnlyDictionary<int, HashSet<int>>> GetSkillIdsByUserIdsAsync(
            IEnumerable<int> userIds,
            CancellationToken cancellationToken = default);
        Task AddAsync(UserSkill userSkill);
        Task DeleteAsync(UserSkill userSkill);
    }
}