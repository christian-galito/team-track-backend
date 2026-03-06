using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TeamTrack.Application.Common.Behaviors;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Users.Commands.DeleteUser;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Tests.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandTest
    {
        private readonly IMediator _mediator;
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public DeleteUserCommandTest()
        {
            var services = new ServiceCollection();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<DeleteUserCommand>();
            });

            services.AddTransient<IUserRepository>(_ => _userRepositoryMock.Object);
            services.AddTransient<IUnitOfWork>(_ => _unitOfWorkMock.Object);

            services.AddValidatorsFromAssemblyContaining<DeleteUserCommand>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task DeleteUser_ShouldFail_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new DeleteUserCommand(1);

            Func<Task> act = () => _mediator.Send(command);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteUser_ShouldSucceed_WhenUserExists()
        {
            var user = CreateUser();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var command = new DeleteUserCommand(1);

            await _mediator.Send(command);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _userRepositoryMock.Verify(x => x.Delete(user), Times.Once);
        }

        private static User CreateUser()
        {
            var user = User.Create(
                firstName: "John",
                middleName: null,
                lastName: "Doe",
                userName: "jdoe",
                email: "john@test.com");

            return user;
        }
    }
}