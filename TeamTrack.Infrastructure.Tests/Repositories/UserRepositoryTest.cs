using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Infrastructure.Repositories;
using TeamTrack.Infrastructure.Tests.Builders;
using TeamTrack.Infrastructure.Tests.Persistence;

namespace TeamTrack.Infrastructure.Tests.Repositories
{
    public class UserRepositoryTest : DbContextTestBase
    {
        [Fact]
        public async Task AddAsync_ShouldPersistUser_WithValidData()
        {
            var role = await new RoleBuilder()
               .WithName("Admin")
               .BuildAndPersistAsync(Context);

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();

            var savedUser = await readOnlyContext.Users.Include(u => u.Credentials).FirstAsync();

            savedUser.Id.Should().NotBe(default);
            savedUser.UserName.Should().Be("jdoe");
            savedUser.Email.Should().Be("john@test.com");
            savedUser.Credentials.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_ShouldPersistUser_WithRolesAndCredentials()
        {
            var role = await new RoleBuilder()
             .WithName("Admin")
             .BuildAndPersistAsync(Context);

            var user = new UserBuilder()
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();

            var savedUser = await readOnlyContext.Users
                .Include(u => u.Roles)
                .Include(u => u.Credentials)
                .FirstAsync();

            savedUser.Id.Should().NotBe(default);
            savedUser.Roles.Should().ContainSingle(r => r.RoleId == role.Id);
            savedUser.Credentials.Should().ContainSingle(c => c.HashedPassword == "hashed-password");
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var role = await new RoleBuilder()
             .WithName("Admin")
             .BuildAndPersistAsync(Context);

            var user1 = new UserBuilder()
                .WithEmail("john@test.com")
                .WithUserName("jdoe1")
                .WithRole(role.Id)
                .Build();

            var user2 = new UserBuilder()
                .WithEmail("john@test.com")
                .WithUserName("jdoe2")
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user1, CancellationToken.None);
            await Context.SaveChangesAsync();

            await repository.AddAsync(user2, CancellationToken.None);

            Func<Task> act = async () => await Context.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenUsernameAlreadyExists()
        {
            var role = await new RoleBuilder()
             .WithName("Admin")
             .BuildAndPersistAsync(Context);

            var user1 = new UserBuilder()
                .WithEmail("john1@test.com")
                .WithUserName("jdoe")
                .WithRole(role.Id)
                .Build();

            var user2 = new UserBuilder()
                .WithEmail("john2@test.com")
                .WithUserName("jdoe")
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user1, CancellationToken.None);
            await Context.SaveChangesAsync();

            await repository.AddAsync(user2, CancellationToken.None);

            Func<Task> act = async () => await Context.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }
}
