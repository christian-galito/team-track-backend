using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Services.RefreshTokenService;

namespace TeamTrack.Infrastructure.Tests.Services.RefreshTokenServices
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IRefreshTokenHasher> _refreshTokenHasherMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<RefreshTokenService>> _loggerMock = new();

        private RefreshTokenService CreateService()
        {
            return new RefreshTokenService(
                _currentUserServiceMock.Object,
                _refreshTokenRepositoryMock.Object,
                _refreshTokenHasherMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public void CreateRefreshToken_ShouldAddTokenToUser()
        {
            var user = CreateUser();
            var service = CreateService();

            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _currentUserServiceMock
                .Setup(x => x.IpAddress)
                .Returns("127.0.0.1");

            _currentUserServiceMock
                .Setup(x => x.UserAgent)
                .Returns("test-agent");

            var result = service.CreateRefreshToken(user);

            result.Should().Be("refresh-token");
            user.RefreshTokens.Should().ContainSingle();
            user.RefreshTokens.First().Token.Should().Be("hashed-refresh-token");
            user.RefreshTokens.First().IpAddress.Should().Be("127.0.0.1");
            user.RefreshTokens.First().UserAgent.Should().Be("test-agent");
        }

        [Fact]
        public void CreateRefreshToken_ShouldRevokeOldestToken_WhenMaxActiveReached()
        {
            var user = CreateUser();
            var service = CreateService();

            for (int i = 0; i < 5; i++)
            {
                user.AddRefreshToken($"refresh-token-{i}", "127.0.0.1", "test-agent");
            }

            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _currentUserServiceMock
                .Setup(x => x.IpAddress)
                .Returns("127.0.0.1");

            _currentUserServiceMock
                .Setup(x => x.UserAgent)
                .Returns("test-agent");

            var result = service.CreateRefreshToken(user);

            result.Should().Be("refresh-token");
            user.RefreshTokens.Count.Should().Be(6); 
            user.RefreshTokens.Count(t => t.IsActive()).Should().Be(5);
            user.RefreshTokens.Any(t => t.IsRevoked).Should().BeTrue();
            user.RefreshTokens.Where(t => t.IsActive()).Should().ContainSingle(t => t.Token == "hashed-refresh-token");
        }

        [Fact]
        public async Task GetValidTokenAsync_ShouldReturnNull_WhenTokenNotFound()
        {
            var service = CreateService();

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            var result = await service.GetValidTokenAsync("refresh-token");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetValidTokenAsync_ShouldRevokeAllTokens_WhenTokenInactive()
        {
            var user = CreateUser();
            var service = CreateService();
            var token = new RefreshToken(
                user: user, 
                refreshToken: "hashed-refresh-token",
                expiresAt: DateTime.UtcNow.AddDays(-1),
                ipAddress: "127.0.0.1",
                userAgent: "test-agent");

            _refreshTokenHasherMock
               .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
               .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var result = await service.GetValidTokenAsync("refresh-token");

            result.Should().BeNull();
            _refreshTokenRepositoryMock.Verify(x => x.GetActiveTokensByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync("hashed-refresh-token", It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetValidTokenAsync_ShouldLogIpMismatch()
        {
            var user = CreateUser();
            var service = CreateService();
            var token = new RefreshToken(
                user: user,
                refreshToken: "hashed-refresh-token",
                expiresAt: DateTime.UtcNow.AddMinutes(10),
                ipAddress: "1.1.1.1",
                userAgent: "test-agent");

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _currentUserServiceMock
                .Setup(x => x.IpAddress)
                .Returns("2.2.2.2");

            _currentUserServiceMock
                .Setup(x => x.UserAgent)
                .Returns("test-agent");

            var result = await service.GetValidTokenAsync("refresh-token");

            result.Should().Be(token);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("IP mismatch")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetValidTokenAsync_ShouldLogUserAgentMismatch()
        {
            var user = CreateUser();
            var service = CreateService();

            var token = new RefreshToken(
                user: user,
                refreshToken: "hashed-refresh-token",
                expiresAt: DateTime.UtcNow.AddMinutes(10),
                ipAddress: "127.0.0.1",
                userAgent: "old-agent");

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _currentUserServiceMock
                .Setup(x => x.IpAddress)
                .Returns("127.0.0.1");

            _currentUserServiceMock
                .Setup(x => x.UserAgent)
                .Returns("new-agent");

            await service.GetValidTokenAsync("refresh-token");

            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UserAgent mismatch")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RevokeAllUserTokensAsync_ShouldRevokeTokens()
        {
            var user = CreateUser();
            var service = CreateService();
            var tokens = new List<RefreshToken>
            {
                new RefreshToken(
                user: user,
                refreshToken: "hashed-refresh-token-1",
                expiresAt: DateTime.UtcNow.AddMinutes(10),
                ipAddress: "1.1.1.1",
                userAgent: "test-agent"),
                new RefreshToken(
                user: user,
                refreshToken: "hashed-refresh-token-2",
                expiresAt: DateTime.UtcNow.AddMinutes(10),
                ipAddress: "1.1.1.1",
                userAgent: "test-agent")
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetActiveTokensByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokens);

            var result = await service.RevokeAllUserTokensAsync(1, "test-reason");

            result.Should().BeEquivalentTo(tokens);
            result.All(t => !t.IsActive()).Should().BeTrue();
            _refreshTokenRepositoryMock.Verify(x => x.GetActiveTokensByUserIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void RotateRefreshToken_ShouldRevokeOldTokenAndAddNew()
        {
            var user = CreateUser();
            var service = CreateService();
            var oldToken = new RefreshToken(
                user: user,
                refreshToken: "hashed-refresh-token",
                expiresAt: DateTime.UtcNow.AddMinutes(10),
                ipAddress: "127.0.0.1",
                userAgent: "test-agent");

            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("new-hashed-refresh-token");

            _currentUserServiceMock
                .Setup(x => x.IpAddress)
                .Returns("127.0.0.1");

            _currentUserServiceMock
                .Setup(x => x.UserAgent)
                .Returns("test-agent");

            var result = service.RotateRefreshToken(oldToken, user);

            result.Should().Be("new-refresh-token");
            oldToken.IsActive().Should().BeFalse();
            user.RefreshTokens.Should().ContainSingle(t => t.Token == "new-hashed-refresh-token");
        }

        private static User CreateUser()
        {
            return User.Create(
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "jdoe",
                email: "john@test.com");
        }
    }
}