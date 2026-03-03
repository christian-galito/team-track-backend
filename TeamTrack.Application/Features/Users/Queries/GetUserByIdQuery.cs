using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Users.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Users.Queries
{
    public record GetUserByIdQuery(int UserId) : IRequest<UserResponseDto>
    {
        public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto>
        {
            private readonly IUserRepository _userRepository;

            public GetUserByIdQueryHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                return new UserResponseDto
                (
                    userId: user.Id,
                    firstName: user.FirstName,
                    middleName: user.MiddleName,
                    lastName: user.LastName,
                    userName: user.UserName,
                    email: user.Email
                );
            }
        }
    }
}
