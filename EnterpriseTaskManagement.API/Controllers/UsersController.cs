using EnterpriseTaskManagement.Application.DTOs.Auth;
using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO?>> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(user);
        }
        
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO?>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(CreateUserRequest request)
        {
            var user = await _userService.AddUserAsync(request);

            return CreatedAtAction(
                    nameof(GetUser),
                    new { id = user.Id },
                    user
                );
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);

            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAt,
                    Path = "/api/users"
                });

            return Ok(new LoginResponse
            {
                AccessToken = result.AccessToken,
                AccessTokenExpiresAt = result.AccessTokenExpiresAt
            });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token is missing.");
            }

            var result = await _userService.RefreshTokenAsync(refreshToken);

            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAt,
                    Path = "/api/users"
                });

            return Ok(new LoginResponse
            {
                AccessToken = result.AccessToken,
                AccessTokenExpiresAt = result.AccessTokenExpiresAt
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _userService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete(
                "refreshToken",
                new CookieOptions
                {
                    Path = "/api/users"
                });

            return NoContent();
        }
    }
}
