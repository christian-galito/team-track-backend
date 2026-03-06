using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Users.Queries;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Users.Queries
{
    public class GetUserByIdQueryTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        public GetUserByIdQueryTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetUserByIdQuery>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task GetUserById_ShouldFail_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var query = new GetUserByIdQuery(1);

            Func<Task> act = () => _mediator.Send(query);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact] 
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            var user = User.Create
            (
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "jdoe",
                email: "john@test.com"
            );

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var query = new GetUserByIdQuery(1);
            var result = await _mediator.Send(query);

            result.Should().NotBeNull();
            result.UserName.Should().Be(user.UserName);
            result.Email.Should().Be(user.Email);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
