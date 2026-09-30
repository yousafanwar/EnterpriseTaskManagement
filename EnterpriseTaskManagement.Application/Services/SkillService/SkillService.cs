using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _repository;

        public SkillService(ISkillRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SkillDto>> GetSkillsAsync()
        {
            var skills = await _repository.GetAllAsync();

            return skills.Select(MapToDto);
        }

        public async Task<SkillDto?> GetSkillAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);

            if (skill == null)
                return null;

            return MapToDto(skill);
        }

        public async Task<SkillDto> AddSkillAsync(CreateSkillRequest request)
        {
            var existingSkill = await _repository.GetByNameAsync(request.Name);

            if (existingSkill != null)
                throw new InvalidOperationException(
                    "A skill with this name already exists.");

            var skill = new Skill(request.Name);

            await _repository.AddAsync(skill);

            return MapToDto(skill);
        }

        public async Task<bool> DeleteSkillAsync(int id)
        {
            var skill = await _repository.GetByIdAsync(id);

            if (skill == null)
                return false;

            await _repository.DeleteAsync(skill);

            return true;
        }

        private static SkillDto MapToDto(Skill skill)
        {
            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            };
        }
    }
}