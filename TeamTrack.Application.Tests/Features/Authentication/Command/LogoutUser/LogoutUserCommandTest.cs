using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Features.Authentication.Commands.LogoutUser;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Tests.Features.Authentication.Command.LogoutUser
{
    public class LogoutUserCommandTest
    {
        private readonly IMediator _mediator;

        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();

        public LogoutUserCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<LogoutUserCommand>();
            });


            services.AddTransient<ICurrentUserService>(_ => _currentUserServiceMock.Object);
            services.AddTransient<IRefreshTokenService>(_ => _refreshTokenServiceMock.Object);


            services.AddValidatorsFromAssemblyContaining<LogoutUserCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task LogoutUser_ShouldFail_WhenUserIdIsInvalid()
        {
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns("jdoe");
            
            Func<Task> act = () => _mediator.Send(new LogoutUserCommand());

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task LogoutUser_ShouldFail_WhenUserIdIsEmpty()
        {
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns("");

            Func<Task> act = () => _mediator.Send(new LogoutUserCommand());

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LogoutUser_ShouldFail_WhenUserIdIsZero()
        {
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns("0");

            Func<Task> act = () => _mediator.Send(new LogoutUserCommand());

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LogoutUser_ShouldFail_WhenUserIdIsNull()
        {
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns((string?)null);

            Func<Task> act = () => _mediator.Send(new LogoutUserCommand());

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LogoutUser_ShouldRevokeTokens_WhenUserIsValid()
        {
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns("1");

            var command = new LogoutUserCommand();

            await _mediator.Send(command);

            _refreshTokenServiceMock.Verify(x =>
                x.RevokeAllUserTokensAsync(1, "Logout", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
