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

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var repository = new UserRepository(Context);

            var result = await repository.GetByIdAsync(999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = new UserBuilder()
                .Build();
            
            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var repository = new UserRepository(readOnlyContext);

            var retrievedUser = await repository.GetByIdAsync(user.Id, CancellationToken.None);

            retrievedUser.Should().NotBeNull();
            retrievedUser!.Email.Should().Be(user.Email);
            retrievedUser!.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeRole()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

            var user = new UserBuilder()
                .WithRole(role.Id)
                .Build();

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var repository = new UserRepository(readOnlyContext);

            var retrievedUser = await repository.GetByIdAsync(user.Id, CancellationToken.None);

            retrievedUser.Should().NotBeNull();
            retrievedUser!.Roles.Should().ContainSingle(r => r.RoleId == role.Id);
        }

        [Fact]
        public async Task Update_ShouldPersist_WhenUserExists()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .Build();

            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            var userId = user.Id;

            using var updateContext = CreateNewContext();
            var repository = new UserRepository(updateContext);

            var loadedUser = await repository.GetByIdAsync(userId, CancellationToken.None);
            loadedUser.Should().NotBeNull();
            loadedUser!.ChangeName("Jane", "Doe", null);
            loadedUser.ChangeEmail("jane@test.com");
            loadedUser.ChangeUserName("jane_doe");

            repository.Update(loadedUser);
            await updateContext.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repoRead = new UserRepository(readContext);
            var updatedUser = await repoRead.GetByIdAsync(userId, CancellationToken.None);

            updatedUser.Should().NotBeNull();
            updatedUser!.FirstName.Should().Be("Jane");
            updatedUser.LastName.Should().Be("Doe");
            updatedUser.MiddleName.Should().BeNull();
            updatedUser.Email.Should().Be("jane@test.com");
            updatedUser.UserName.Should().Be("jane_doe");
        }

        [Fact]
        public async Task Update_ShouldNotAffectOtherUsers_WhenOneUserIsUpdated()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

            var user1 = new UserBuilder()
                .WithUserName("user1")
                .WithEmail("user1@test.com")
                .WithRole(role.Id)
                .Build();
            var user2 = new UserBuilder()
                .WithUserName("user2")
                .WithEmail("user2@test.com")
                .WithRole(role.Id)
                .Build();

            Context.Users.Add(user1);
            Context.Users.Add(user2);
            await Context.SaveChangesAsync();
            var user1Id = user1.Id;
            var user2Id = user2.Id;

            using var updateContext = CreateNewContext();
            var repository = new UserRepository(updateContext);
            var loadedUser1 = await repository.GetByIdAsync(user1Id, CancellationToken.None);
            loadedUser1!.ChangeUserName("user1_updated");
            repository.Update(loadedUser1);
            await updateContext.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repoRead = new UserRepository(readContext);
            var unchangedUser = await repoRead.GetByIdAsync(user2Id, CancellationToken.None);
            unchangedUser.Should().NotBeNull();
            unchangedUser!.UserName.Should().Be("user2");
        }


        [Fact]
        public async Task Delete_ShouldRemoveUser_WhenUserExists()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .Build();

            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            var userId = user.Id;

            using var deleteContext = CreateNewContext();
            var repository = new UserRepository(deleteContext);
            var toDelete = await repository.GetByIdAsync(userId, CancellationToken.None);
            toDelete.Should().NotBeNull();

            repository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repoRead = new UserRepository(readContext);
            var deletedUser = await repoRead.GetByIdAsync(userId, CancellationToken.None);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldNotAffectOtherUsers_WhenOneUserIsDeleted()
        {
            var role = await new RoleBuilder()
                .WithName("Admin")
                .BuildAndPersistAsync(Context);

            var user1 = new UserBuilder()
                .WithUserName("user1")
                .WithEmail("user1@test.com")
                .WithRole(role.Id)
                .Build();
            var user2 = new UserBuilder()
                .WithUserName("user2")
                .WithEmail("user2@test.com")
                .WithRole(role.Id)
                .Build();

            Context.Users.Add(user1);
            Context.Users.Add(user2);
            await Context.SaveChangesAsync();
            var user1Id = user1.Id;
            var user2Id = user2.Id;

            using var deleteContext = CreateNewContext();
            var repository = new UserRepository(deleteContext);
            var toDelete = await repository.GetByIdAsync(user1Id, CancellationToken.None);
            repository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var repoRead = new UserRepository(readContext);
            var remainingUser = await repoRead.GetByIdAsync(user2Id, CancellationToken.None);
            remainingUser.Should().NotBeNull();
            remainingUser!.UserName.Should().Be("user2");
        }
    }
}
