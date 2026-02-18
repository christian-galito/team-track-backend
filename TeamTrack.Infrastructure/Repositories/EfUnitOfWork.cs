using TeamTrack.Application.Interfaces;
using TeamTrack.Infrastructure.Interfaces;

namespace TeamTrack.Infrastructure.Repositories
{
    internal class EfUnitOfWork : IUnitOfWork
    {
        private readonly ITeamTrackDbContext _context;

        public EfUnitOfWork(ITeamTrackDbContext context) 
        {
            _context = context;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
