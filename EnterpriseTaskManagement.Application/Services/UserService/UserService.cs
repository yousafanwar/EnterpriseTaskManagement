using EnterpriseTaskManagement.Application.DTOs.User;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository UserRepository)
        {
            _userRepository = UserRepository;
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
                Email = user.Email
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
                Email = user.Email
            });
        }
        public async Task<UserDTO> AddUserAsync(CreateUserRequest request)
        {
            var user = new User(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.RoleId
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
    }
}
