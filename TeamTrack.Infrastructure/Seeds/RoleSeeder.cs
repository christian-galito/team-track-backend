using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Interfaces;
using TeamTrack.Infrastructure.Seeds.Dto;

namespace TeamTrack.Infrastructure.Seeds
{
    public class RoleSeeder : ISeeder
    {
        private static readonly RoleSeedDto[] Roles = new[]
        {
            new RoleSeedDto(1, "Administrator", "Full system access"),
            new RoleSeedDto(2, "Manager", "Manages teams and projects"),
            new RoleSeedDto(3, "Employee", "Regular user with basic access"),
        };


        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                Roles.Select(r => new { r.Id, r.Name, r.Description }));
        }
    }
}
