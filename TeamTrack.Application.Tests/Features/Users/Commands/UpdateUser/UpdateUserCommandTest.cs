using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Users.Commands.UpdateUser;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public UpdateUserCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<UpdateUserCommand>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<UpdateUserCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task UpdateUser_ShouldFail_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = CreateValidCommand();

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateUser_ShouldFail_WhenEmailAlreadyExists()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = CreateValidCommand();

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ShouldFail_WhenUserNameAlreadyExists()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = CreateValidCommand();

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<ValidationException>();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdateUser_WhenCommandIsValid()
        {
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.UserNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateUser());

            var command = CreateValidCommand();

            command = command with { Email = "JOHN@TEST.COM" };

            var result = await _mediator.Send(command);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            result.Email.Should().Be("john@test.com");
            result.UserName.Should().Be("jdoe");
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

        public static UpdateUserCommand CreateValidCommand()
        {
            return new UpdateUserCommand(
                UserId: 1,
                FirstName: "John",
                MiddleName: null,
                LastName: "Doe",
                UserName: "jdoe",
                Email: "john@test.com"
            );
        }
    }
}
