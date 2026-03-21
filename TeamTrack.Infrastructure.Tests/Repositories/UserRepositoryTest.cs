using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Security;
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
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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

            var repository = new UserRepository(Context); 

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);
            var retrievedUser = await readRepository.GetByIdAsync(user.Id, CancellationToken.None);

            retrievedUser.Should().NotBeNull();
            retrievedUser!.Email.Should().Be(user.Email);
            retrievedUser.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeRole()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);

            var retrievedUser = await readRepository.GetByIdAsync(user.Id, CancellationToken.None);

            retrievedUser.Should().NotBeNull();
            retrievedUser!.Roles.Should().ContainSingle(r => r.RoleId == role.Id);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ShouldReturnAllPermissions_WhenUserIsAdmin()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);

            var retrievedPermissions = await readRepository.GetUserPermissionsAsync(user.Id, CancellationToken.None);

            retrievedPermissions.Should().NotBeNull();
            retrievedPermissions.Should().BeEquivalentTo(Permissions.All.Select(p => p.Name));
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ShouldReturnEmployeePermissions_WhenUserIsEmployee()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Employee");

            var user = new UserBuilder()
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);

            var retrievedPermissions = await readRepository.GetUserPermissionsAsync(user.Id, CancellationToken.None);

            var employeePermissions = new List<PermissionDefinition>()
            {
                Permissions.User.Read,
                Permissions.User.Update,
            };

            employeePermissions.AddRange(Permissions.Project.All);

            retrievedPermissions.Should().NotBeNull();
            retrievedPermissions.Should().BeEquivalentTo(employeePermissions.Select(p => p.Name));
        }

        [Fact]
        public async Task Update_ShouldPersist_WhenUserExists()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);

            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();
            var userId = user.Id;

            using var updateContext = CreateNewContext();
            var updateRepository = new UserRepository(updateContext);
            var loadedUser = await updateRepository.GetByIdAsync(userId, CancellationToken.None);
            
            loadedUser.Should().NotBeNull();
            loadedUser!.ChangeName("Jane", "Doe", null);
            loadedUser.ChangeEmail("jane@test.com");
            loadedUser.ChangeUserName("jane_doe");

            updateRepository.Update(loadedUser);
            await updateContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);
            var updatedUser = await readRepository.GetByIdAsync(userId, CancellationToken.None);

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
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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

            var repository = new UserRepository(Context);
            await repository.AddAsync(user1, CancellationToken.None);
            await repository.AddAsync(user2, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var updateContext = CreateNewContext();
            var updateRepository = new UserRepository(updateContext);
            var loadedUser1 = await updateRepository.GetByIdAsync(user1.Id, CancellationToken.None);
            
            loadedUser1!.ChangeUserName("user1_updated");
            updateRepository.Update(loadedUser1);
            await updateContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);
            var unchangedUser = await readRepository.GetByIdAsync(user2.Id, CancellationToken.None);
            
            unchangedUser.Should().NotBeNull();
            unchangedUser!.UserName.Should().Be("user2");
        }

        [Fact]
        public async Task Delete_ShouldRemoveUser_WhenUserExists()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

            var user = new UserBuilder()
                .WithUserName("jdoe")
                .WithEmail("john@test.com")
                .WithRole(role.Id)
                .Build();

            var repository = new UserRepository(Context);
            await repository.AddAsync(user, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var deleteContext = CreateNewContext();
            var deleteRepository = new UserRepository(deleteContext);
            var toDelete = await deleteRepository.GetByIdAsync(user.Id, CancellationToken.None);
            
            toDelete.Should().NotBeNull();

            deleteRepository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);
            var deletedUser = await readRepository.GetByIdAsync(user.Id, CancellationToken.None);

            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldNotAffectOtherUsers_WhenOneUserIsDeleted()
        {
            var role = await Context.Roles.FirstAsync(r => r.Name == "Administrator");

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

            var repository = new UserRepository(Context);
            await repository.AddAsync(user1, CancellationToken.None);
            await repository.AddAsync(user2, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var deleteContext = CreateNewContext();
            var deleteRepository = new UserRepository(deleteContext);
            var toDelete = await deleteRepository.GetByIdAsync(user1.Id, CancellationToken.None);

            deleteRepository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new UserRepository(readOnlyContext);
            var remainingUser = await readRepository.GetByIdAsync(user2.Id, CancellationToken.None);
            
            remainingUser.Should().NotBeNull();
            remainingUser!.UserName.Should().Be("user2");
        }
    }
}
