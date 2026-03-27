using Microsoft.Extensions.Logging;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Services.RefreshTokenService
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _maxActiveTokens = 5;

        public RefreshTokenService(
            ICurrentUserService currentUserService,
            IRefreshTokenRepository repository,
            IRefreshTokenHasher refreshTokenHasher,
            ITokenService tokenService,
            ILogger<RefreshTokenService> logger,
            IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _refreshTokenRepository = repository;
            _refreshTokenHasher = refreshTokenHasher;
            _tokenService = tokenService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public string CreateRefreshToken(User user, string? ipAddress = null, string? userAgent = null)
        {
            var activeTokens = user.RefreshTokens.Where(t => t.IsActive()).ToList();
        
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashedRefreshToken = _refreshTokenHasher.HashRefreshToken(refreshToken);

            if (activeTokens.Count >= _maxActiveTokens)
            {
                var oldest = activeTokens.OrderBy(t => t.ExpiresAt).First();
                oldest.Revoke(hashedRefreshToken, "MaxTokensLimitReached");
            }

            user.AddRefreshToken(hashedRefreshToken, ipAddress ?? _currentUserService.IpAddress, userAgent ?? _currentUserService.UserAgent);

            return refreshToken;
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var hashedToken = _refreshTokenHasher.HashRefreshToken(token);
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken, cancellationToken);

            if (refreshToken == null)
                return null;

            if (!refreshToken.IsActive())
            {
                await RevokeAllUserTokensAsync(refreshToken.UserId, "InactiveTokenUsage", cancellationToken);
                return null;
            }

            if (!string.IsNullOrEmpty(_currentUserService.IpAddress) && refreshToken.IpAddress != _currentUserService.IpAddress)
            {
                _logger.LogWarning("IP mismatch on token rotation. TokenId={TokenId}, OldIP={OldIP}, NewIP={NewIP}",
                   refreshToken.Id,
                   refreshToken.IpAddress,
                   _currentUserService.IpAddress);
            }

            if (!string.IsNullOrEmpty(_currentUserService.UserAgent) && refreshToken.UserAgent != _currentUserService.UserAgent)
            {
                _logger.LogWarning("UserAgent mismatch on token rotation. TokenId={TokenId}, OldUA={OldUA}, NewUA={NewUA}",
                    refreshToken.Id,
                    refreshToken.UserAgent,
                    _currentUserService.UserAgent);
            }

            return refreshToken;
        }

        public async Task<IEnumerable<RefreshToken>> RevokeAllUserTokensAsync(int userId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);
            foreach (var t in tokens)
            {
                t.Revoke(null, reason);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return tokens;
        }

        public string RotateRefreshToken(RefreshToken oldToken, User user)
        {
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashedRefreshToken = _refreshTokenHasher.HashRefreshToken(refreshToken);
            
            oldToken.Revoke(hashedRefreshToken, "Rotated");
            
            user.AddRefreshToken(hashedRefreshToken, _currentUserService.IpAddress,_currentUserService.UserAgent);

            return refreshToken;
        }
    }
}