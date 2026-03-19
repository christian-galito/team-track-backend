using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Authentication.Commands.TokenRefresh;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Authentication.Command.TokenRefresh
{
    public class TokenRefreshCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRefreshTokenHasher> _refreshTokenHasherMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public TokenRefreshCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<TokenRefreshCommand>();
            });

            services.AddTransient<IRefreshTokenRepository>(_ => _refreshTokenRepositoryMock.Object);
            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<ITokenService>(_ => _tokenServiceMock.Object);
            services.AddTransient<IRefreshTokenHasher>(_ => _refreshTokenHasherMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<TokenRefreshCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task TokenRefresh_ShouldFail_WhenTokenIsEmpty()
        {
            var command = new TokenRefreshCommand("");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _refreshTokenHasherMock.Verify(x => x.HashRefreshToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TokenRefresh_ShouldFail_WhenTokenIsExpiredOrInvalid()
        {
            var invalidToken = RefreshToken.Create(CreateUser(), "invalid-refresh-token");

            invalidToken.Revoke();

            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidToken);

            var command = new TokenRefreshCommand("invalid-refresh-token");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task TokenRefresh_ShouldFail_WhenTokenIsNotFound()
        {
            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            var command = new TokenRefreshCommand("invalid-refresh-token");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<NotFoundException>();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TokenRefresh_ShouldGiveRefreshAndAccessToken_WhenTokenIsValid()
        {
            var user = CreateUser();
            var tokenFromCommand = "valid-refresh-token-from-command";
            var storedHashedToken = "valid-stored-hashed-refresh-token";
            var hashedNewRefreshToken = "hashed-new-refresh-token";

            var validToken = RefreshToken.Create(user, storedHashedToken);


            _refreshTokenHasherMock
                .Setup(x => x.HashRefreshToken(It.IsAny<string>()))
                .Returns((string token) =>
                 {
                     if (token == tokenFromCommand)
                         return storedHashedToken;      
                     if (token == "valid-refresh-token")
                         return hashedNewRefreshToken;  
                     return token;                      
                 });

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validToken);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

             _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("valid-access-token");

            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("valid-refresh-token");


            var command = new TokenRefreshCommand(tokenFromCommand);
            var result = await _mediator.Send(command);

            validToken.IsRevoked.Should().BeTrue();
            result.AccessToken.Should().Be("valid-access-token");
            result.RefreshToken.Should().Be("valid-refresh-token");
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
