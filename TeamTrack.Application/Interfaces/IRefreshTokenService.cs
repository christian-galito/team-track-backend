using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        string CreateRefreshToken(User user, string? ipAddress = null, string? userAgent = null);

        string RotateRefreshToken(RefreshToken oldToken, User user);
        
        Task<IEnumerable<RefreshToken>> RevokeAllUserTokensAsync(int userId, string? reason = null, CancellationToken cancellationToken = default);

        Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
