using TeamTrack.Domain.Entities;
using TeamTrack.Infrastructure.Persistence;

namespace TeamTrack.Infrastructure.Tests.Builders
{
    public class RoleBuilder
    {
        private string _name = "User";

        public RoleBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public async Task<Role> BuildAndPersistAsync(TeamTrackDbContext context)
        {
            var role = new Role(_name);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            return role;
        }
    }
}
