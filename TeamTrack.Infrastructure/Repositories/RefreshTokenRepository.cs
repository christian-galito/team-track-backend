using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Interfaces;

namespace TeamTrack.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ITeamTrackDbContext _context;

        public RefreshTokenRepository(ITeamTrackDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r =>  r.Token == refreshToken, cancellationToken);
        }
    }
}
