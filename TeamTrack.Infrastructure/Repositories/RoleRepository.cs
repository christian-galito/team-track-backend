using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        public readonly ITeamTrackDbContext _context;

        public RoleRepository(ITeamTrackDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(int roleId, CancellationToken cancellationToken)
        {
            return await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
        }
    }
}
