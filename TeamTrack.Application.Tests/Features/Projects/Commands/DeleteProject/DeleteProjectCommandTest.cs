using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Projects.Commands.DeleteProject;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;
using FluentValidation;

namespace TeamTrack.Application.Tests.Features.Projects.Commands.DeleteProject
{
    public class DeleteProjectCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public DeleteProjectCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<DeleteProjectCommand>();
            });

            services.AddTransient<IProjectRepository>(_ => _projectRepositoryMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<DeleteProjectCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task DeleteProject_ShouldFail_WhenProjectDoesNotExist()
        {
            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var command = new DeleteProjectCommand(1);

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteProject_ShouldSucceed_WhenProjectExists()
        {
            var project = new Project("Test project");

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var command = new DeleteProjectCommand(1);

            await _mediator.Send(command);

            _projectRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _projectRepositoryMock.Verify(x => x.Delete(project), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

