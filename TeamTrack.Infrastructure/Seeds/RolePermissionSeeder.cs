using Microsoft.EntityFrameworkCore;
using TeamTrack.Domain.Entities;
using TeamTrack.Domain.Security;
using TeamTrack.Infrastructure.Interfaces;
using TeamTrack.Infrastructure.Seeds.Dto;

namespace TeamTrack.Infrastructure.Seeds
{
    public class RolePermissionSeeder : ISeeder
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            var rolePermissions = new List<RolePermissionSeedDto>();

            // Admin
            rolePermissions.AddRange(Permissions.All.Select(p => new RolePermissionSeedDto(1, p.Id)));

            // Manager
            rolePermissions.AddRange(Permissions.All.Select(p => new RolePermissionSeedDto(2, p.Id)));

            // Employee
            rolePermissions.Add(new RolePermissionSeedDto(3, Permissions.User.Read.Id));
            rolePermissions.Add(new RolePermissionSeedDto(3, Permissions.User.Update.Id));
            rolePermissions.AddRange(Permissions.Project.All.Select(p => new RolePermissionSeedDto(3, p.Id)));

            modelBuilder.Entity<RolePermission>().HasData(
                rolePermissions.Select(rp => new { rp.RoleId, rp.PermissionId })
            );

        }
    }
}
