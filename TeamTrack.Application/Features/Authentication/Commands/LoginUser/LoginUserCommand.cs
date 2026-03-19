using MediatR;
using TeamTrack.Application.Features.Authentication.Responses;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Authentication.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<TokenResponse>
    {
        public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, TokenResponse>
        {
            private readonly IUserRepository _userRepository;

            private readonly IPasswordHasher _passwordHasher;

            private readonly ITokenService _tokenService;

            private readonly IRefreshTokenHasher _refreshTokenHasher;

            private readonly IUnitOfWork _unitOfWork;

            public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenHasher refreshTokenHasher, IUnitOfWork unitOfWork)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _tokenService = tokenService;
                _refreshTokenHasher = refreshTokenHasher;
                _unitOfWork = unitOfWork;
            }

            public async Task<TokenResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

                if (user is null ||
                    !_passwordHasher.VerifyPassword(
                        user.Credentials.SingleOrDefault()?.HashedPassword ?? string.Empty,
                        request.Password))
                {
                    throw new UnauthorizedAccessException("Invalid credentials.");
                }

                var accessToken = _tokenService.GenerateAccessToken(user.Id, user.UserName, user.Email);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var hashedRefreshToken = _refreshTokenHasher.HashRefreshToken(refreshToken);

                user.AddRefreshToken(hashedRefreshToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new TokenResponse(accessToken, refreshToken);
            }
        }
    }
}
