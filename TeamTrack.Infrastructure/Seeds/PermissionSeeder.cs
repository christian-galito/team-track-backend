using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;
using TeamTrack.Domain.Security;
using TeamTrack.Infrastructure.Interfaces;

namespace TeamTrack.Infrastructure.Seeds
{
    public class PermissionSeeder : ISeeder
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasData(
                Permissions.All.Select(p => new { p.Id, p.Name, p.Description })
            );
        }
    }
}
