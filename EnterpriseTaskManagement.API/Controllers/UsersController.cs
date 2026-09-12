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
            var user = await _userService.LoginAsync(request);

            return Ok(user);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken(RefreshTokenRequest request)
        {
                var response = await _userService.RefreshTokenAsync(request);

                return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            await _userService.LogoutAsync(request.RefreshToken);

            return NoContent();
        }
    }
}
