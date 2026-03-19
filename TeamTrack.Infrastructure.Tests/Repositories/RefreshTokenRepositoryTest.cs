using FluentAssertions;
using TeamTrack.Infrastructure.Repositories;
using TeamTrack.Infrastructure.Tests.Builders;
using TeamTrack.Infrastructure.Tests.Persistence;

namespace TeamTrack.Infrastructure.Tests.Repositories
{
    public class RefreshTokenRepositoryTests : DbContextTestBase
    {
        [Fact]
        public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

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
