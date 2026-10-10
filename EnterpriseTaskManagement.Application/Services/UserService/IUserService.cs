using EnterpriseTaskManagement.Application.DTOs.Auth;
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
        Task<UserDTO?> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<UserDTO?> UpdateProfileAsync(int id, UpdateProfileRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId);
        Task<bool> AddUserSkillAsync(int userId, int skillId);
        Task<bool> RemoveUserSkillAsync(int userId, int skillId);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<LoginResult> RefreshTokenAsync(string request);
        Task LogoutAsync(string refreshToken);
        Task ResetPasswordAsync(int userId, ResetPasswordRequest request);
        Task AdminResetPasswordAsync(int userId, AdminResetPasswordRequest request);
    }
}
