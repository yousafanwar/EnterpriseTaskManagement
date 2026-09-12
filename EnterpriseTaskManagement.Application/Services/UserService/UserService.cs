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

        public UserService(
            IUserRepository UserRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _userRepository = UserRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
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
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
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

            return new LoginResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<LoginResponse> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            var refreshTokenHash = _refreshTokenService.HashRefreshToken(request.RefreshToken);

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

            return new LoginResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash);

            if (storedToken == null)
            {
                return;
            }

            storedToken.Revoke();

            await _refreshTokenRepository.UpdateAsync(storedToken);
        }
    }
}
