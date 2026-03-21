using MediatR;
using TeamTrack.Application.Common.Exceptions;
using TeamTrack.Application.Features.Authentication.Responses;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Features.Authentication.Commands.TokenRefresh
{
    public record TokenRefreshCommand(string Token) : IRequest<TokenResponse>
    {
        public class TokenRefreshCommandHandler : IRequestHandler<TokenRefreshCommand, TokenResponse>
        {
            private readonly IRefreshTokenRepository _refreshTokenRepository;

            private readonly IUserRepository _userRepository;

            private readonly ITokenService _tokenService;

            private readonly IRefreshTokenHasher _refreshTokenHasher;

            private readonly IUnitOfWork _unitOfWork;

            public TokenRefreshCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, ITokenService tokenService, IRefreshTokenHasher refreshTokenHasher, IUnitOfWork unitOfWork)
            {
                _refreshTokenRepository = refreshTokenRepository;
                _userRepository = userRepository;
                _tokenService = tokenService;
                _refreshTokenHasher = refreshTokenHasher;
                _unitOfWork = unitOfWork;
             }

            public async Task<TokenResponse> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
            {
                var hashedToken = _refreshTokenHasher.HashRefreshToken(request.Token);
                var existingToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken, cancellationToken);

                if (existingToken == null)
                {
                    throw new NotFoundException(nameof(RefreshToken));
                }

                if (!existingToken.IsActive())
                {
                    throw new UnauthorizedAccessException("Invalid refresh token.");
                }

                var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), existingToken.UserId);
                }

                var accessToken = await _tokenService.GenerateAccessToken(user.Id, user.UserName, user.Email, cancellationToken);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var hashedNewRefreshToken = _refreshTokenHasher.HashRefreshToken(refreshToken);

                user.AddRefreshToken(hashedNewRefreshToken);
                existingToken.Revoke(hashedNewRefreshToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new TokenResponse(accessToken, refreshToken);
            }
        }

    }
}
