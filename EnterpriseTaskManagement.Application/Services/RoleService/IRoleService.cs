using EnterpriseTaskManagement.Application.DTOs.Role;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services.RoleService
{
    public interface IRoleService
    {
        Task<RoleDTO?> GetRoleAsync(int id);
        Task<IEnumerable<RoleDTO>> GetRolesAsync();
        Task<RoleDTO> AddRoleAsync(CreateRoleRequest request);
    }
}
