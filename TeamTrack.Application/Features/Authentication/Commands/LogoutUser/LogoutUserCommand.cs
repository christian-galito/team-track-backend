using MediatR;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Application.Features.Authentication.Commands.LogoutUser
{
    public record LogoutUserCommand() : IRequest;

    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
    {
        private readonly ICurrentUserService _currentUserService;

        private readonly IRefreshTokenService _refreshTokenService;

        public LogoutUserCommandHandler(ICurrentUserService currentUserService, IRefreshTokenService refreshTokenService)
        {
            _currentUserService = currentUserService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUserService.UserId) || _currentUserService.UserId == "0")
            {
                throw new UnauthorizedAccessException("Cannot logout anonymous user.");
            }

            if (!int.TryParse(_currentUserService.UserId, out var userId))
            {
                throw new InvalidOperationException("Invalid user ID.");
            }

            await _refreshTokenService.RevokeAllUserTokensAsync(userId, "Logout", cancellationToken);
        }
    }
}
