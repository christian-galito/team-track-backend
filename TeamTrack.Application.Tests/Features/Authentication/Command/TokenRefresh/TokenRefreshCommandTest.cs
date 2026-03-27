using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Features.Authentication.Commands.TokenRefresh;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Authentication.Command.TokenRefresh
{
    public class TokenRefreshCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public TokenRefreshCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<TokenRefreshCommand>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<ITokenService>(_ => _tokenServiceMock.Object);
            services.AddTransient<IRefreshTokenService>(_ => _refreshTokenServiceMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<TokenRefreshCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task TokenRefresh_ShouldFail_WhenTokenIsInvalid()
        {
            _refreshTokenServiceMock
                .Setup(x => x.GetValidTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?) null);

            var command = new TokenRefreshCommand("invalid-refresh-token");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TokenRefresh_ShouldGiveRefreshAndAccessToken_WhenTokenIsValid()
        {
            var user = CreateUser();
            var tokenFromCommand = "valid-refresh-token-from-command";
            var storedHashedToken = "valid-stored-hashed-refresh-token";

            var validToken = RefreshToken.Create(user, storedHashedToken);

            _refreshTokenServiceMock
                .Setup(x => x.GetValidTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validToken);
                
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

             _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("valid-access-token");

            _refreshTokenServiceMock
                .Setup(x => x.RotateRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<User>()))
                .Returns("valid-refresh-token");

            var command = new TokenRefreshCommand(tokenFromCommand);
            var result = await _mediator.Send(command);

            result.AccessToken.Should().Be("valid-access-token");
            result.RefreshToken.Should().Be("valid-refresh-token");
            _refreshTokenServiceMock.Verify(x => x.RotateRefreshToken(validToken, user), Times.Once);
            _refreshTokenServiceMock.Verify(x => x.GetValidTokenAsync(tokenFromCommand, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static User CreateUser()
        {
            var user = User.Create(
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "johndoe",
                email: "john@test.com");

            return user;
        }
    }
}
