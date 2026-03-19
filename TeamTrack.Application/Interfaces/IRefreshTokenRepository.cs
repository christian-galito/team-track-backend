using TeamTrack.Domain.Entities;

namespace TeamTrack.Application.Interfaces
{
    public interface IRefreshTokenRepository : IRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
