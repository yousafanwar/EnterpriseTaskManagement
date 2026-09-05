using EnterpriseTaskManagement.Application.DTOs.Role;
using EnterpriseTaskManagement.Application.Services.RoleService;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDTO?>> GetRole(int id)
        {
            var role = await _roleService.GetRoleAsync(id);
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDTO>> CreateRole(CreateRoleRequest request)
        {
            var role = await _roleService.AddRoleAsync(request);
            return CreatedAtAction(
                nameof(GetRole),
                new { id = role.Id },
                role
            );
        }
    }
}
