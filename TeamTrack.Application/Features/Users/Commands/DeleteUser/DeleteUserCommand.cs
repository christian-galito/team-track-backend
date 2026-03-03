using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(int UserId): IRequest
    {
        public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
        {
            private readonly IUserRepository _userRepository;

            private readonly IUnitOfWork _unitOfWork;

            public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
            {
                _userRepository = userRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                _userRepository.Delete(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
