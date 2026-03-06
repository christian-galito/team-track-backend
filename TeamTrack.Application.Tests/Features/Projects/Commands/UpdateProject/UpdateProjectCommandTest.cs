using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Projects.Commands.UpdateProject;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public UpdateProjectCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<UpdateProjectCommand>();
            });

            services.AddTransient<IProjectRepository>(_ => _projectRepositoryMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<UpdateProjectCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task UpdateProject_ShouldFail_WhenProjectDoesNotExist()
        {
            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var command = new UpdateProjectCommand(1, "Updated project");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<NotFoundException>();

            _projectRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _projectRepositoryMock.Verify(x => x.Update(It.IsAny<Project>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProject_ShouldUpdateProject_WhenCommandIsValid()
        {
            var project = new Project("Test project");

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var command = new UpdateProjectCommand(1, "Updated project");

            var result = await _mediator.Send(command);

            _projectRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            result.Name.Should().Be("Updated project");
        }
    }
}
