using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskDbContext _dbContext;
        public UserRepository(TaskDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _dbContext.Users.FindAsync(id);
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }
        public async Task AddNewAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task UpdateAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> HasCommentsAsync(int userId)
        {
            return await _dbContext.Comments.AnyAsync(comment => comment.UserId == userId);
        }

        public async Task DeleteAsync(User user)
        {
            var memberships = await _dbContext.ProjectUsers
                .Where(pu => pu.UserId == user.Id)
                .ToListAsync();

            _dbContext.ProjectUsers.RemoveRange(memberships);
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
