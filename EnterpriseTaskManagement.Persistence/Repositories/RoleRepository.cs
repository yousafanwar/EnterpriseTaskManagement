using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly TaskDbContext _dbContext;
        public RoleRepository(TaskDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role?> GetByIdAsync(int id) 
        { 
            return await _dbContext.Roles.FindAsync(id); 
        }
        public async Task<IEnumerable<Role>> GetAllAsync() 
        { 
            return await _dbContext.Roles.ToListAsync(); 
        }
        public async Task<Role> CreateAsync(Role role) 
        { 
            await _dbContext.Roles.AddAsync(role); 
            await _dbContext.SaveChangesAsync(); 
            return role; 
        }
    }
}