using EnterpriseTaskManagement.Application.DTOs;

namespace EnterpriseTaskManagement.Application.Interfaces.Services
{
    public interface ISkillService
    {
        Task<IEnumerable<SkillDto>> GetSkillsAsync();
        Task<SkillDto?> GetSkillAsync(int id);
        Task<SkillDto> AddSkillAsync(CreateSkillRequest request);
        Task<bool> DeleteSkillAsync(int id);
    }
}