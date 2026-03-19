using TeamTrack.Domain.Common;

namespace TeamTrack.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int Id { get; private set; }

        public string Token { get; private set; } = null!;

        public int UserId { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsRevoked { get; private set; }

        public string? ReplacedByToken { get; private set; }

        public virtual User User { get; set; } = null!;

        private RefreshToken() { }

        private RefreshToken(User user, string refreshToken)
        {

            ValidateAndSetRefreshToken(user, refreshToken);
        }

        private void ValidateAndSetRefreshToken(User user, string refreshToken)
        {
            if (user == null)
            {
                throw new DomainException("User cannot be null");
            }

            User = user;
            Token = refreshToken;
            ExpiresAt = DateTime.UtcNow.AddDays(7);
            IsRevoked = false;
        }

        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            ReplacedByToken = replacedByToken;
            ExpiresAt = DateTime.UtcNow;
        }

        public bool IsActive() => !IsRevoked && DateTime.UtcNow <= ExpiresAt;

        internal static RefreshToken Create(User user, string refreshToken)
        {

            return new RefreshToken(user, refreshToken);
        }
    }
}
