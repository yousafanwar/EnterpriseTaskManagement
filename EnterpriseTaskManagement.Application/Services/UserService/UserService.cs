using EnterpriseTaskManagement.Application.DTOs.Auth;
using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services.AuthService;
using EnterpriseTaskManagement.Application.Services.Interfaces;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserSkillRepository _userSkillRepository;
        private readonly ISkillRepository _skillRepository;

        public UserService(
            IUserRepository UserRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IRoleRepository roleRepository,
            IUserSkillRepository userSkillRepository,
            ISkillRepository skillRepository
            )
        {
            _userRepository = UserRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _roleRepository = roleRepository;
            _userSkillRepository = userSkillRepository;
            _skillRepository = skillRepository;
        }

        public async Task<UserDTO?> GetUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if(user == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }
        public async Task<IEnumerable<UserDTO?>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId
            });
        }
        public async Task<UserDTO> AddUserAsync(CreateUserRequest request)
        {
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.RoleId,
                    hashPassword
                );

            await _userRepository.AddNewAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

        }
        public async Task<UserDTO?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            var email = request.Email.Trim();
            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException("Another user already uses this email.");
            }

            if (await _roleRepository.GetByIdAsync(request.RoleId) == null)
            {
                throw new ArgumentException("Role does not exist.");
            }

            user.Update(request.FirstName.Trim(), request.LastName.Trim(), email, request.RoleId);
            await _userRepository.UpdateAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }

        public async Task<UserDTO?> UpdateProfileAsync(int id, UpdateProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            user.Update(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                user.Email,
                user.RoleId);

            await _userRepository.UpdateAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }

        public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var userSkills = await _userSkillRepository.GetByUserIdAsync(userId);

            return userSkills.Select(us => new UserSkillDto
            {
                SkillId = us.SkillId,
                SkillName = us.Skill?.Name ?? string.Empty
            });
        }

        public async Task<bool> AddUserSkillAsync(int userId, int skillId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null)
            {
                throw new ArgumentException("Skill does not exist.");
            }

            var existing = await _userSkillRepository.GetAsync(userId, skillId);
            if (existing != null)
            {
                return false;
            }

            await _userSkillRepository.AddAsync(new UserSkill(userId, skillId));
            return true;
        }

        public async Task<bool> RemoveUserSkillAsync(int userId, int skillId)
        {
            var userSkill = await _userSkillRepository.GetAsync(userId, skillId);
            if (userSkill == null)
            {
                return false;
            }

            await _userSkillRepository.DeleteAsync(userSkill);
            return true;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return false;
            }

            if (await _userRepository.HasCommentsAsync(id))
            {
                throw new InvalidOperationException(
                    "This user has authored comments and cannot be deleted.");
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }
        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            var refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

            var refreshTokenEntity = new RefreshToken(
                    refreshTokenHash,
                    DateTime.UtcNow.AddDays(7),
                    user.Id
                );

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return new LoginResult
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<LoginResult> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            var user = storedToken.User;

            // Revoke old refresh token
            storedToken.Revoke();

            await _refreshTokenRepository.UpdateAsync(storedToken);

            // Generate new refresh token
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

            var newRefreshTokenHash = _refreshTokenService.HashRefreshToken(newRefreshToken);

            var newRefreshTokenEntity = new RefreshToken(
                newRefreshTokenHash,
                DateTime.UtcNow.AddDays(7),
                user.Id
            );

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

            // Generate new access token
            var accessToken = _jwtTokenService.GenerateToken(user);

            return new LoginResult
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var refreshTokenHash =
                _refreshTokenService.HashRefreshToken(refreshToken);

            var storedToken =
                await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash);

            if (storedToken == null)
            {
                return;
            }

            storedToken.Revoke();

            await _refreshTokenRepository.UpdateAsync(storedToken);
        }
    }
}
