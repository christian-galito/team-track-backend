using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IRefreshTokenRepository : IRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken);

        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken);
    }
}
