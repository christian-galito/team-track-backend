using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Projects.Queries;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Projects.Queries
{
    public class GetProjectByIdQueryTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IProjectRepository> _projectRepositoryMock = new();

        public GetProjectByIdQueryTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetProjectByIdQuery>();
            });

            services.AddTransient<IProjectRepository>(_ => _projectRepositoryMock.Object);

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task GetProjectById_ShouldFail_WhenProjectDoesNotExist()
        {
            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var query = new GetProjectByIdQuery(1);

            Func<Task> act = () => _mediator.Send(query);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnProject_WhenProjectExists()
        {
            var project = new Project("Test project");

            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var query = new GetProjectByIdQuery(1);
            var result = await _mediator.Send(query);

            result.Should().NotBeNull();
            result.Name.Should().Be(project.Name);
            _projectRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
