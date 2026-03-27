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

        public DateTime? RevokedAt { get; private set; }

        public string? RevokedBy { get; private set; }

        public string? ReplacedByToken { get; private set; }

        public string? IpAddress { get; private set; }

        public string? UserAgent { get; private set; }

        public virtual User User { get; set; } = null!;

        private RefreshToken() { }

        private RefreshToken(User user, string refreshToken, string? ipAddress = null, string? userAgent = null)
        {

            ValidateAndSetRefreshToken(user, refreshToken, ipAddress, userAgent);
        }

        private void ValidateAndSetRefreshToken(User user, string refreshToken, string? ipAddress = null, string? userAgent = null)
        {
            if (user == null)
            {
                throw new DomainException("User cannot be null");
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new DomainException("Refresh token cannot be null.");
            }

            User = user;
            Token = refreshToken;
            ExpiresAt = DateTime.UtcNow.AddDays(7);
            IsRevoked = false;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }

        public void Revoke(string? replacedByToken = null, string? revokedBy = null)
        {
            IsRevoked = true;
            ReplacedByToken = replacedByToken;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
        }

        public bool IsActive() => !IsRevoked && DateTime.UtcNow <= ExpiresAt;

        internal static RefreshToken Create(User user, string refreshToken, string? ipAddress = null, string? userAgent = null)
        {

            return new RefreshToken(user, refreshToken, ipAddress, userAgent);
        }

        internal RefreshToken(User user, string refreshToken, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
        {
            ValidateAndSetRefreshToken(user, refreshToken, ipAddress, userAgent);
            ExpiresAt = expiresAt;
        }
    }
}
