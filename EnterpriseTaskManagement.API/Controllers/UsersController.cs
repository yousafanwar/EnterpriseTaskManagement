using EnterpriseTaskManagement.Application.DTOs.Auth;
using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<UserDTO?>> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(user);
        }
        
        
        [HttpGet]
        [Authorize]
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

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserRequest request)
        {
            var user = await _userService.UpdateUserAsync(id, request);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> UpdateMyProfile(UpdateProfileRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserId, out var userId))
                return Unauthorized();

            var user = await _userService.UpdateProfileAsync(userId, request);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("{id:int}/skills")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserSkillDto>>> GetUserSkills(int id)
        {
            if (!CanManageUserSkills(id))
                return Forbid();

            try
            {
                var skills = await _userService.GetUserSkillsAsync(id);
                return Ok(skills);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id:int}/skills/{skillId:int}")]
        [Authorize]
        public async Task<IActionResult> AddUserSkill(int id, int skillId)
        {
            if (!CanManageUserSkills(id))
                return Forbid();

            try
            {
                var added = await _userService.AddUserSkillAsync(id, skillId);
                if (!added)
                    return Conflict("Skill is already assigned to this user.");

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}/skills/{skillId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveUserSkill(int id, int skillId)
        {
            if (!CanManageUserSkills(id))
                return Forbid();

            var removed = await _userService.RemoveUserSkillAsync(id, skillId);

            if (!removed)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id.ToString())
                return BadRequest("You cannot delete your own account.");

            var deleted = await _userService.DeleteUserAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        private bool CanManageUserSkills(int targetUserId)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return currentUserId == targetUserId.ToString();
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
