using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Repositories;
using TeamTrack.Infrastructure.Tests.Persistence;

namespace TeamTrack.Infrastructure.Tests.Repositories
{
    public class ProjectRepositoryTest : DbContextTestBase
    {
        [Fact]
        public async Task AddAsync_ShouldPersistProject_WithValidData()
        {
            var project = new Project("Test project");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();

            var savedProject = await readOnlyContext.Projects.FirstAsync();

            savedProject.Id.Should().NotBe(default);
            savedProject.Name.Should().Be("Test project");
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenNameAlreadyExistsForActiveProject()
        {
            var project1 = new Project("Project");
            var project2 = new Project("Project");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project1, CancellationToken.None);
            await Context.SaveChangesAsync();

            await repository.AddAsync(project2, CancellationToken.None);

            Func<Task> act = async () => await Context.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
        {
            var repository = new ProjectRepository(Context);
            var result = await repository.GetByIdAsync(999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProject_WhenProjectExists()
        {
            var project = new Project("Existing project");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);
            var retrievedProject = await readRepository.GetByIdAsync(project.Id, CancellationToken.None);

            retrievedProject.Should().NotBeNull();
            retrievedProject!.Name.Should().Be("Existing project");
        }

        [Fact]
        public async Task GetAsync_ShouldReturnProjects()
        {
            var project1 = new Project("Project 1");
            var project2 = new Project("Project 2");
            var project3 = new Project("Project 3");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project1, CancellationToken.None);
            await repository.AddAsync(project2, CancellationToken.None);
            await repository.AddAsync(project3, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);

            var retrievedProjects = await readRepository.GetAsync(CancellationToken.None);
            retrievedProjects.Should().HaveCount(3);
        }

        [Fact]
        public async Task Update_ShouldPersist_WhenProjectExists()
        {
            var project = new Project("Initial Project Name");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var updateContext = CreateNewContext();
            var updateRepository = new ProjectRepository(updateContext);
            var loadedProject = await updateRepository.GetByIdAsync(project.Id, CancellationToken.None);

            loadedProject.Should().NotBeNull();
            loadedProject!.ChangeName("Updated name");

            updateRepository.Update(loadedProject);
            await updateContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);
            var updatedProject = await readRepository.GetByIdAsync(project.Id, CancellationToken.None);

            updatedProject.Should().NotBeNull();
            updatedProject!.Name.Should().Be("Updated name");
        }

        [Fact]
        public async Task Update_ShouldNotAffectOtherProjects_WhenOneProjectIsUpdated()
        { 
            var project1 = new Project("Project 1");
            var project2 = new Project("Project 2");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project1, CancellationToken.None);
            await repository.AddAsync(project2, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var updateContext = CreateNewContext();
            var updateRepository = new ProjectRepository(updateContext);
            var loadedProject1 = await updateRepository.GetByIdAsync(project1.Id, CancellationToken.None);

            loadedProject1!.ChangeName("Project 1 updated");
            updateRepository.Update(loadedProject1);
            await updateContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);
            var unchangedProject = await readRepository.GetByIdAsync(project2.Id, CancellationToken.None);

            unchangedProject.Should().NotBeNull();
            unchangedProject!.Name.Should().Be("Project 2");
        }

        [Fact]
        public async Task Delete_ShouldRemoveProject_WhenProjectExists()
        {
            var project = new Project("Deletable project");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var deleteContext = CreateNewContext();
            var deleteRepository = new ProjectRepository(deleteContext);
            var toDelete = await deleteRepository.GetByIdAsync(project.Id, CancellationToken.None);

            toDelete.Should().NotBeNull();

            deleteRepository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);
            var deletedProject = await readRepository.GetByIdAsync(project.Id, CancellationToken.None);

            deletedProject.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldNotAffectOtherProjects_WhenOneProjectIsDeleted()
        {
            var project1 = new Project("Project 1");
            var project2 = new Project("Project 2");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project1, CancellationToken.None);
            await repository.AddAsync(project2, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var deleteContext = CreateNewContext();
            var deleteRepository = new ProjectRepository(deleteContext);
            var toDelete = await deleteRepository.GetByIdAsync(project1.Id, CancellationToken.None);

            toDelete.Should().NotBeNull();

            deleteRepository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();
            
            using var readOnlyContext = CreateNewContext();
            var readRepository = new ProjectRepository(readOnlyContext);
            var remainingProject = await readRepository.GetByIdAsync(project2.Id, CancellationToken.None);
            
            remainingProject.Should().NotBeNull();
            remainingProject!.Name.Should().Be("Project 2");
        }


        [Fact]
        public async Task Delete_ShouldSoftDeleteProject_AndKeepItOutOfQueries()
        {
            var project = new Project("Soft deletable project");

            var repository = new ProjectRepository(Context);
            await repository.AddAsync(project, CancellationToken.None);
            await Context.SaveChangesAsync();

            using var deleteContext = CreateNewContext();
            var deleteRepository = new ProjectRepository(deleteContext);
            var toDelete = await deleteRepository.GetByIdAsync(project.Id, CancellationToken.None);
            toDelete.Should().NotBeNull();

            deleteRepository.Delete(toDelete!);
            await deleteContext.SaveChangesAsync();

            using var readContext = CreateNewContext();
            var readRepository = new ProjectRepository(readContext);
            var deletedProject = await readRepository.GetByIdAsync(project.Id, CancellationToken.None);

            deletedProject.Should().BeNull();
        }
    }
}

