using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Features.Authentication.Commands.LoginUser;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Authentication.Command.LoginUser
{
    public class LoginUserCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

        public LoginUserCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<LoginUserCommand>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<IPasswordHasher>(_ => _passwordHasherMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);
            services.AddTransient<ITokenService>(_ => _tokenServiceMock.Object);
            services.AddTransient<IRefreshTokenService>(_ => _refreshTokenServiceMock.Object);
            services.AddTransient<IHttpContextAccessor>(_ => _httpContextAccessorMock.Object);

            services.AddValidatorsFromAssemblyContaining<LoginUserCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task LoginUser_ShouldFail_WhenCredentialsAreInvalid()
        {
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateUserWithCredential());

            _passwordHasherMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var command = new LoginUserCommand("test@test.com", "invalid-password");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("")]
        public async Task LoginUser_ShouldFail_WhenEmailIsInvalid(string email)
        {

            var command = new LoginUserCommand(email, "invalid-password");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task LoginUser_ShouldFail_WhenPasswordIsInvalid()
        {
            var command = new LoginUserCommand("test@test.com", "");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task LoginUser_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            var user = CreateUserWithCredential();
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("valid-access-token");

            _refreshTokenServiceMock
                .Setup(x => x.CreateRefreshToken(It.IsAny<User>(), It.IsAny<string?>(), It.IsAny<string>()))
                .Returns("valid-refresh-token");

            var command = new LoginUserCommand("test@test.com", "validpassword");
            var result = await _mediator.Send(command);

            _refreshTokenServiceMock.Verify(x => x.CreateRefreshToken(user, It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
            result.AccessToken.Should().Be("valid-access-token");
            result.RefreshToken.Should().Be("valid-refresh-token");
        }

        private static User CreateUserWithCredential()
        {
            var user = User.Register("John", null, "Doe", "jdoe", "john@test.com", "hashedpassword");

            return user;
        }
    }
}
