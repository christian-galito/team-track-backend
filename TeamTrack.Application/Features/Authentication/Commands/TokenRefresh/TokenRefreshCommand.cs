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
            private readonly IUserRepository _userRepository;

            private readonly ITokenService _tokenService;

            private readonly IRefreshTokenService _refreshTokenService;           
            

            private readonly IUnitOfWork _unitOfWork;

            public TokenRefreshCommandHandler
            (
                IUserRepository userRepository,
                ITokenService tokenService,
                IRefreshTokenService refreshTokenService,
                IUnitOfWork unitOfWork
            )
            {
                _userRepository = userRepository;
                _tokenService = tokenService;
                _refreshTokenService = refreshTokenService;
                _unitOfWork = unitOfWork;
             }

            public async Task<TokenResponse> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
            {
                var existingToken = await _refreshTokenService.GetValidTokenAsync(request.Token, cancellationToken);

                if (existingToken == null)
                {
                    throw new UnauthorizedAccessException("Invalid refresh token.");
                }

                var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), existingToken.UserId);
                }

                var accessToken = await _tokenService.GenerateAccessToken(user.Id, user.UserName, user.Email, cancellationToken);
                var refreshToken = _refreshTokenService.RotateRefreshToken(existingToken, user);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new TokenResponse(accessToken, refreshToken);
            }
        }

    }
}
