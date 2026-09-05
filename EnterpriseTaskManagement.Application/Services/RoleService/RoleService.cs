using EnterpriseTaskManagement.Application.DTOs.Role;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services.RoleService
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleDTO?> GetRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
                return null;

            return new RoleDTO
            {
                Id = role.Id,
                Name = role.Name
            };
        }

        public async Task<IEnumerable<RoleDTO>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            return roles.Select(role => new RoleDTO
            {
                Id = role.Id,
                Name = role.Name
            });
        }

        public async Task<RoleDTO> AddRoleAsync(CreateRoleRequest request)
        {
            var role = new Role(request.Name);

            var createdRole = await _roleRepository.CreateAsync(role);

            return new RoleDTO
            {
                Id = createdRole.Id,
                Name = createdRole.Name
            };
        }
    }
}
