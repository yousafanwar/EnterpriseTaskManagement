using EnterpriseTaskManagement.Application.DTOs;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/skills")]
    [Authorize]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
        {
            var skills = await _skillService.GetSkillsAsync();

            return Ok(skills);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SkillDto>> GetSkill(int id)
        {
            var skill = await _skillService.GetSkillAsync(id);

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SkillDto>> CreateSkill(
            CreateSkillRequest request)
        {
            var skill = await _skillService.AddSkillAsync(request);

            return CreatedAtAction(
                nameof(GetSkill),
                new { id = skill.Id },
                skill);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var deleted = await _skillService.DeleteSkillAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}