using EnterpriseTaskManagement.Application.DTOs.Auth;
using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services.AuthService;
using EnterpriseTaskManagement.Application.Services.Interfaces;
using EnterpriseTaskManagement.Application.Services.UserService;
using EnterpriseTaskManagement.Domain.Entities;
using Moq;

namespace EnterpriseTaskManagement.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _jwtTokenServiceMock.Object,
                _refreshTokenServiceMock.Object,
                _refreshTokenRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetUserAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserAsync_UserExists_ReturnsUserDTO()
        {
            // Arrange
            var user = new User(
                "Hatake",
                "Kakashi",
                "hk@example.com",
                1,
                "hashed-password"
            );

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("Hatake", result.FirstName);
            Assert.Equal("Kakashi", result.LastName);
            Assert.Equal("hk@example.com", result.Email);
            Assert.Equal(1, result.RoleId);
        }

        [Fact]
        public async Task GetUsersAsync_UsersExist_ReturnsUserDTOs()
        {
            // Arrange
            var users = new List<User>
    {
        new User(
            "Hatake",
            "Kakashi",
            "hk@example.com",
            1,
            "hashed-password"
        ),
        new User(
            "Uchiha",
            "Madara",
            "mu@example.com",
            2,
            "hashed-password"
        )
    };

            _userRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userService.GetUsersAsync();

            // Assert
            Assert.NotNull(result);

            var resultList = result.ToList();

            Assert.Equal(2, resultList.Count);

            Assert.Equal("Hatake", resultList[0].FirstName);
            Assert.Equal("Kakashi", resultList[0].LastName);
            Assert.Equal("hk@example.com", resultList[0].Email);
            Assert.Equal(1, resultList[0].RoleId);

            Assert.Equal("Uchiha", resultList[1].FirstName);
            Assert.Equal("Madara", resultList[1].LastName);
            Assert.Equal("mu@example.com", resultList[1].Email);
            Assert.Equal(2, resultList[1].RoleId);
        }

        [Fact]
        public async Task AddUserAsync_ValidRequest_CreatesUserAndReturnsDTO()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                FirstName = "Hatake",
                LastName = "Kakashi",
                Email = "hk@example.com",
                RoleId = 1,
                Password = "Password@123"
            };

            _userRepositoryMock
                .Setup(x => x.AddNewAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.AddUserAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hatake", result.FirstName);
            Assert.Equal("Kakashi", result.LastName);
            Assert.Equal("hk@example.com", result.Email);

            _userRepositoryMock.Verify(
                x => x.AddNewAsync(It.Is<User>(user =>
                    user.FirstName == "Hatake" &&
                    user.LastName == "Kakashi" &&
                    user.Email == "hk@example.com" &&
                    user.RoleId == 1
                )),
                Times.Once
            );

            _userRepositoryMock.Verify(
                x => x.AddNewAsync(It.Is<User>(user =>
                    BCrypt.Net.BCrypt.Verify(
                        "Password@123",
                        user.PasswordHash
        )
    )),
    Times.Once
);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "unknown@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync(request)
            );

            Assert.Equal("Invalid email or password.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            const string correctPassword = "CorrectPassword123!";
            const string wrongPassword = "WrongPassword123!";

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            var user = new User(
                "Hatake",
                "Kakashi",
                "hk@example.com",
                1,
                passwordHash
            );

            var request = new LoginRequest
            {
                Email = "hk@example.com",
                Password = wrongPassword
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync(request)
            );

            Assert.Equal("Invalid email or password.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            const string password = "CorrectPassword123!";
            const string refreshToken = "refresh-token";
            const string refreshTokenHash = "refresh-token-hash";
            const string accessToken = "access-token";

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User(
                "Hatake",
                "Kakashi",
                "hk@example.com",
                1,
                passwordHash
            );

            var request = new LoginRequest
            {
                Email = "hk@example.com",
                Password = password
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtTokenServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns(accessToken);

            _refreshTokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns(refreshToken);

            _refreshTokenServiceMock
                .Setup(x => x.HashRefreshToken(refreshToken))
                .Returns(refreshTokenHash);

            _refreshTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accessToken, result.AccessToken);
            Assert.Equal(refreshToken, result.RefreshToken);

            _jwtTokenServiceMock.Verify(
                x => x.GenerateToken(user),
                Times.Once
            );

            _refreshTokenServiceMock.Verify(
                x => x.GenerateRefreshToken(),
                Times.Once
            );

            _refreshTokenServiceMock.Verify(
                x => x.HashRefreshToken(refreshToken),
                Times.Once
            );

            _refreshTokenRepositoryMock.Verify(
                x => x.AddAsync(It.Is<RefreshToken>(token =>
                    token.UserId == user.Id
                )),
                Times.Once
            );
        }
    }
}