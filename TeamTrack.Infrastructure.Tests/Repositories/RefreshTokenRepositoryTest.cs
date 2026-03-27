using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Repositories;
using TeamTrack.Infrastructure.Tests.Builders;
using TeamTrack.Infrastructure.Tests.Persistence;

namespace TeamTrack.Infrastructure.Tests.Repositories
{
    public class RefreshTokenRepositoryTests : DbContextTestBase
    {
        [Fact]
        public async Task GetActiveTokensByUserIdAsync_ShouldReturnOnlyActiveTokens()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .WithRefreshToken("active-token-1")
                .WithRefreshToken("active-token-2")
                .WithRefreshToken("revoked-token")
                .Build();

            var revokedToken = user.RefreshTokens.First(t => t.Token == "revoked-token");
            revokedToken.Revoke();

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repository = new RefreshTokenRepository(readContext);

            var result = await repository.GetActiveTokensByUserIdAsync(user.Id, CancellationToken.None);

            result.Should().HaveCount(2);
            result.All(t => t.IsActive()).Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveTokensByUserIdAsync_ShouldExcludeExpiredTokens()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRefreshToken("active-token")
                .WithRole(role.Id)
                .Build();

            var expiredToken = new RefreshToken(
                user,
                "expired-token",
                DateTime.UtcNow.AddHours(-1));

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repository = new RefreshTokenRepository(readContext);

            var result = await repository.GetActiveTokensByUserIdAsync(user.Id, CancellationToken.None);

            result.Should().HaveCount(1);
            result.First().Token.Should().Be("active-token");
        }

        [Fact]
        public async Task GetActiveTokensByUserIdAsync_ShouldReturnOnlyUserTokens()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user1 = new UserBuilder()
                .WithUserName("user1")
                .WithEmail("user1@test.com")
                .WithRole(role.Id)
                .WithRefreshToken("token-user1")
                .Build();

            var user2 = new UserBuilder()
                .WithUserName("user2")
                .WithEmail("user2@test.com")
                .WithRole(role.Id)
                .WithRefreshToken("token-user2")
                .Build();

            Context.Users.AddRange(user1, user2);
            await Context.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repository = new RefreshTokenRepository(readContext);

            var result = await repository.GetActiveTokensByUserIdAsync(user1.Id, CancellationToken.None);

            result.Should().HaveCount(1);
            result.First().UserId.Should().Be(user1.Id);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .WithRefreshToken("hashed-refresh-token")
                .Build();

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repository = new RefreshTokenRepository(readContext);

            var result = await repository.GetByTokenAsync("hashed-refresh-token", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Token.Should().Be("hashed-refresh-token");
            result.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            var repository = new RefreshTokenRepository(Context);

            var result = await repository.GetByTokenAsync("non-existent-token", CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
