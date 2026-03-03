using FluentAssertions;
using FluentValidation;
using MediatR;
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

            var command = new LoginUserCommand("test@test.com", "invalidpassword");

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();

        }

        [Theory]
        [InlineData("invalidemail")]
        [InlineData("")]
        public async Task LoginUser_ShouldFail_WhenEmailIsInvalid(string email)
        {

            var command = new LoginUserCommand(email, "invalidpassword");

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
        public async Task LoginUser_ShouldReturnUser_WhenCredentialsAreValid()
        {
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateUserWithCredential());

            _passwordHasherMock
                .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var command = new LoginUserCommand("test@test.com", "invalidpassword");
            var result = await _mediator.Send(command);

            result.UserName.Should().Be("jdoe");
        }

        private static User CreateUserWithCredential()
        {
            var user = User.Register("John", null, "Doe", "jdoe", "john@test.com", "hashedpassword");

            return user;
        }
    }
}
