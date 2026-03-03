using MediatR;
using TeamTrack.Application.Features.Authentication.Responses;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Authentication.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>
    {
        public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
        {
            private readonly IUserRepository _userRepository;

            private readonly IPasswordHasher _passwordHasher;

            public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
            }

            public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

                if (user is null ||
                    !_passwordHasher.VerifyPassword(
                        user.Credentials.SingleOrDefault()?.HashedPassword ?? string.Empty,
                        request.Password))
                {
                    throw new UnauthorizedAccessException("Invalid credentials.");
                }

                return new LoginUserResponse(user.Id, user.UserName);
            }
        }
    }
}
