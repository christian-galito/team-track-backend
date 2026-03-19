using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Services.Authentication
{
    public class RefreshTokenHasher : IRefreshTokenHasher
    {
        private readonly JwtSettings _settings;

        public RefreshTokenHasher(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string HashRefreshToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_settings.RefreshTokenHashKey);
            using var hmac = new HMACSHA256(key);

            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hash = hmac.ComputeHash(tokenBytes);

            return Convert.ToBase64String(hash);
        }
    }
}