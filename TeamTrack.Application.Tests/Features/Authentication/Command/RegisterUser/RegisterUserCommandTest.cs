using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Features.Authentication.Commands.RegisterUser;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Authentication.Command.RegisterUser
{
    public class RegisterUserCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public RegisterUserCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<IRoleRepository>(_ => _roleRepositoryMock.Object);
            services.AddTransient<IPasswordHasher>(_ => _passwordHasherMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<RegisterUserCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task RegisterUser_ShouldFail_WhenEmailAlreadyExists()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("john@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new RegisterUserCommand(
                FirstName: "John",
                MiddleName: null,
                LastName: "Doe",
                UserName: "jdoe",
                Email: "john@test.com",
                Password: "password",
                RoleId: 1
            );

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Never);
        }

        [Fact]
        public async Task RegisterUser_ShouldFail_WhenUserNameExists()
        {
            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync("jdoe", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new RegisterUserCommand(
             FirstName: "John",
             MiddleName: null,
             LastName: "Doe",
             UserName: "jdoe",
             Email: "john@test.com",
             Password: "password",
             RoleId: 1
            );

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUser_ShouldCreateUser_WhenCommandIsValid()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roleRepositoryMock
                .Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _passwordHasherMock
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed-password");

            var command = ValidCommand();
            var result = await _mediator.Send(command);

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify( x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            result.UserName.Should().Be("jdoe");
            result.Email.Should().Be("john@test.com");
        }

        [Theory]
        [InlineData("short")]
        [InlineData("alllowercase")]
        [InlineData("ALLUPPERCASE")]
        [InlineData("12345678")]
        [InlineData("N0SpecialCharacter")]
        public async Task RegisterUser_ShouldFail_WhenPasswordNotInCorrectFormat(string password)
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roleRepositoryMock
                .Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _passwordHasherMock
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed-password");

            var command = new RegisterUserCommand(
              FirstName: "John",
              MiddleName: null,
              LastName: "Doe",
              UserName: "jdoe",
              Email: "john@test.com",
              Password: password,
              RoleId: 1
             );

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static RegisterUserCommand ValidCommand() => new RegisterUserCommand(
            FirstName: "John",
            MiddleName: null,
            LastName: "Doe",
            UserName: "jdoe",
            Email: "john@test.com",
            Password: "Passw0rd!",
            RoleId: 1);
    }
}
