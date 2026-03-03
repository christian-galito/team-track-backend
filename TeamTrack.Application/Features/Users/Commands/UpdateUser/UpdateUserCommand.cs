using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Users.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Users.Commands.UpdateUser
{
    public record UpdateUserCommand(int UserId, string FirstName, string? MiddleName, string LastName, string Email, string UserName) : IRequest<UserResponseDto>
    {
        public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponseDto>
        {
            private readonly IUserRepository _userRepository;

            private readonly IUnitOfWork _unitOfWork;

            public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
            {
                _userRepository = userRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                user.ChangeName(request.FirstName, request.LastName, request.MiddleName);
                user.ChangeEmail(request.Email);
                user.ChangeUserName(request.UserName);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new UserResponseDto
                (
                    userId: user.Id,
                    firstName: user.FirstName,
                    middleName: user.MiddleName,
                    lastName: user.LastName,
                    email: user.Email,
                    userName: user.UserName
                );
            }
        }
    }
}
