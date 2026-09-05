using EnterpriseTaskManagement.Application.DTOs.Role;
using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface IRoleRepository
    {
        public Task<Role?> GetByIdAsync(int id);
        public Task<IEnumerable<Role>> GetAllAsync();
        public Task<Role> CreateAsync(Role role);
    }
}
