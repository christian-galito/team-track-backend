using MediatR;
using Microsoft.AspNetCore.Http;
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

            private readonly IRefreshTokenService _refreshTokenService;

            private readonly IHttpContextAccessor _httpContextAccessor;

            private readonly IUnitOfWork _unitOfWork;

            public LoginUserCommandHandler
            (
                IUserRepository userRepository,
                IPasswordHasher passwordHasher, 
                ITokenService tokenService, 
                IRefreshTokenService refreshTokenService,
                IHttpContextAccessor httpContextAccesor,
                IUnitOfWork unitOfWork
            )
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _tokenService = tokenService;
                _refreshTokenService = refreshTokenService;
                _httpContextAccessor = httpContextAccesor;
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

                var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

                var accessToken = await _tokenService.GenerateAccessToken(user.Id, user.UserName, user.Email, cancellationToken);
                var refreshToken = _refreshTokenService.CreateRefreshToken(user, ip, userAgent);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new TokenResponse(accessToken, refreshToken);
            }
        }
    }
}
