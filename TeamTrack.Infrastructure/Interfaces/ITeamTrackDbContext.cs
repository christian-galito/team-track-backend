using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Interfaces
{
    public interface ITeamTrackDbContext
    {

        DbSet<Role> Roles { get; }
        DbSet<User> Users { get; }
        DbSet<UserRole> UserRoles { get; }
        DbSet<UserCredential> UserCredentials { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
