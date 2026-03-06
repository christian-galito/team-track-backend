using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Features.Projects.Queries;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Projects.Queries
{
    public class GetProjectsQueryTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IProjectRepository> _projectRepositoryMock = new();

        public GetProjectsQueryTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetProjectsQuery>();
            });

            services.AddTransient<IProjectRepository>(_ => _projectRepositoryMock.Object);

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task GetProjects_ShouldReturnEmpty_WhenNoProjectsExist()
        {
            _projectRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Project>());

            var query = new GetProjectsQuery();

            var result = await _mediator.Send(query);

            result.Should().BeEmpty();
            _projectRepositoryMock.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetProjects_ShouldReturnProjects_WhenProjectsExist()
        {
            var projects = new List<Project>
            {
                new Project("Project 1"),
                new Project("Project 2")
            };

            _projectRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(projects);

            var query = new GetProjectsQuery();

            var result = await _mediator.Send(query);

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Project 1");
            result[1].Name.Should().Be("Project 2");
            _projectRepositoryMock.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
