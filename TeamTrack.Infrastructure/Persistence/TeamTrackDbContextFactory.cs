using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Persistence
{
    public class TeamTrackDbContextFactory : IDesignTimeDbContextFactory<TeamTrackDbContext>
    {
        public TeamTrackDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TeamTrackDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            var dummyUserService = new CurrentUserServiceFake();

            return new TeamTrackDbContext(optionsBuilder.Options, dummyUserService);
        }

        private class CurrentUserServiceFake : ICurrentUserService
        {
            public string? UserId => null;
            public string UserName => "MigrationUser";
        }
    }
}
