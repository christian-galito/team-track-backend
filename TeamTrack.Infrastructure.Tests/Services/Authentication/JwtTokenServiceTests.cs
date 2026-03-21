using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TeamTrack.Application.Interfaces;
using TeamTrack.Infrastructure.Services.Authentication;

namespace TeamTrack.Infrastructure.Tests.Services.Authentication
{
    public class JwtTokenServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock  = new();

        public JwtTokenServiceTests()
        {
            var services = new ServiceCollection();

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
        }

        [Fact]
        public async Task GenerateAccessToken_ShouldIncludePermissionClaims()
        {
            var userId = 1;
            var userName = "johndoe";
            var email = "john@test.com";
            var permissions = new[] { "Read", "Write", "Delete" };

            _userRepositoryMock
                .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            var jwtSettings = Options.Create(new JwtSettings
            {
                SecretKey = "supersecretkey1234567890abcdefg!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            });

            var tokenService = new JwtTokenService(jwtSettings, _userRepositoryMock.Object);

            var tokenString = await tokenService.GenerateAccessToken(userId, userName, email, CancellationToken.None);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(userId.ToString());
            token.Claims.First(c => c.Type == "username").Value.Should().Be(userName);
            token.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be(email);

            var permissionClaims = token.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            permissionClaims.Should().BeEquivalentTo(permissions);
        }
    }
}
