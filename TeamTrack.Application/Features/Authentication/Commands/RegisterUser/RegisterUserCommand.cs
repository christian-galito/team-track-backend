using MediatR;
using TeamTrack.Application.Features.Authentication.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Authentication.Commands.RegisterUser
{
    public record RegisterUserCommand(
        string FirstName,
        string? MiddleName,
        string LastName,
        string UserName,
        string Email,
        string Password,
        int RoleId) 
    : IRequest<RegisterUserResponse>
    {
        public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
        {
            private readonly IPasswordHasher _passwordHasher;
            private readonly IUserRepository _userRepository;
            private readonly IUnitOfWork _unitOfWork;

            public RegisterUserCommandHandler(IPasswordHasher passwordHasher, IUserRepository userRepository, IUnitOfWork unitOfWork)
            {
                _passwordHasher = passwordHasher;
                _userRepository = userRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                var hashedPassword = _passwordHasher.HashPassword(request.Password);

                var user = User.Register(
                    firstName: request.FirstName,
                    middleName: request.MiddleName,
                    lastName: request.LastName,
                    userName: request.UserName,
                    email: request.Email,
                    hashedPassword: hashedPassword
                );

                user.AssignRole(request.RoleId);

                await _userRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new RegisterUserResponse(
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
