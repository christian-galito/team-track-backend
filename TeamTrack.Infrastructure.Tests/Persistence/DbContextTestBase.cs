using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamTrack.Infrastructure.Persistence;
using TeamTrack.Infrastructure.Tests.Services.CurrentUser;

namespace TeamTrack.Infrastructure.Tests.Persistence
{
    public abstract class DbContextTestBase : IDisposable
    {
        protected readonly TeamTrackDbContext Context;

        private readonly SqliteConnection _connection;

        public DbContextTestBase()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<TeamTrackDbContext>()
                .UseSqlite(_connection)
                .Options;

            var _currentUser = new FakeCurrentUserService();

            Context = new TeamTrackDbContext(options, _currentUser);
            Context.Database.EnsureCreated();
        }

        protected TeamTrackDbContext CreateNewContext()
        {
            var options = new DbContextOptionsBuilder<TeamTrackDbContext>()
                .UseSqlite(_connection)
                .Options;

            var freshContext = new TeamTrackDbContext(options, new FakeCurrentUserService());
            freshContext.Database.EnsureCreated();

            return freshContext;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
