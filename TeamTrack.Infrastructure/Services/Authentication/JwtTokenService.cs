using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Services.Authentication
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        
        private readonly IUserRepository _userRepository;

        public JwtTokenService(IOptions<JwtSettings> settings, IUserRepository userRepository)
        {
            _settings = settings.Value;
            _userRepository = userRepository;
        }

        public async Task<string> GenerateAccessToken(int userId, string userName, string email, CancellationToken cancellationToken)
        {
            var permissions = await _userRepository.GetUserPermissionsAsync(userId, cancellationToken);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("username", userName),
                new Claim(ClaimTypes.Email, email),
            };

            claims.AddRange(permissions.Select(p => new Claim("Permission", p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}