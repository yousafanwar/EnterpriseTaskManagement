using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        public int RoleId { get; private set; }
        public Role Role { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        public ICollection<TaskHistory> TaskHistories { get; private set; } = new List<TaskHistory>();
        private User()
            {

            }

        public User(string firstName, string lastName, string email, int roleId, string passwordHash)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            RoleId = roleId;
            PasswordHash = passwordHash;
        }

        public void Update(string firstName, string lastName, string email, int roleId)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            RoleId = roleId;
        }
    }
}
