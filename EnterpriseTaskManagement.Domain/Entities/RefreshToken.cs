namespace EnterpriseTaskManagement.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }

        public string TokenHash { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public bool Revoked { get; private set; }

        public int? UserId { get; private set; }

        public User? User { get; private set; }

        private RefreshToken()
        {
        }

        public RefreshToken(
            string tokenHash,
            DateTime expiresAt,
            int userId)
        {
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            Revoked = false;
            UserId = userId;
        }

        public void Revoke()
        {
            Revoked = true;
        }
    }
}