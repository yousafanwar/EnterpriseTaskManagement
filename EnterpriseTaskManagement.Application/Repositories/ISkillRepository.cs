using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(int id);
        Task<IEnumerable<Skill>> GetAllAsync();
        Task<Skill?> GetByNameAsync(string name);
        Task AddAsync(Skill skill);
        Task DeleteAsync(Skill skill);
    }
}