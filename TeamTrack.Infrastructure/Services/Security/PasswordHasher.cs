using Microsoft.AspNetCore.Identity;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Services.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private static readonly object _dummyUser = new();

        private readonly PasswordHasher<object> _hasher = new();

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            return _hasher.HashPassword(_dummyUser, password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                _dummyUser,
                hashedPassword,
                providedPassword
            );

            return result == PasswordVerificationResult.Success;
        }
    }
}
