using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services.UserService
{
    public interface IUserService
    {
        Task<UserDTO?> GetUserAsync(int id);
        Task<IEnumerable<UserDTO?>> GetUsersAsync();
        Task<UserDTO> AddUserAsync(CreateUserRequest request);
    }
}
