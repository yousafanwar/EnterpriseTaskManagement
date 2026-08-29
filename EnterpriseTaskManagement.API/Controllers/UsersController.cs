using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

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
        public async Task<ActionResult<UserDTO?>> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(user);
        }
        
        
        [HttpGet]
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
    }
}
