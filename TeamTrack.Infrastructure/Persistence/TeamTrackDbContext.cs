using Microsoft.EntityFrameworkCore;
using TeamTrack.Application.Interfaces;
using TeamTrack.Domain.Common;
using TeamTrack.Domain.Entities;
using System.Collections.Concurrent;

namespace TeamTrack.Infrastructure.Persistence
{
    public class TeamTrackDbContext : DbContext, ITeamTrackDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        private static readonly ConcurrentDictionary<string, string> SnakeCaseCache = new();

        public TeamTrackDbContext(DbContextOptions<TeamTrackDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
                {
                    modelBuilder.Entity(entity.ClrType)
                        .Property(nameof(BaseEntity.CreatedDate))
                        .IsRequired();
                }
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCaseCached(entity.GetTableName()!));

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(ToSnakeCaseCached(property.Name));

                foreach (var key in entity.GetKeys())
                    key.SetName(ToSnakeCaseCached(key.GetName()!));

                foreach (var fk in entity.GetForeignKeys())
                    fk.SetConstraintName(ToSnakeCaseCached(fk.GetConstraintName()!));

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(ToSnakeCaseCached(index.GetDatabaseName()!));
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamTrackDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreated(_currentUserService.UserName);
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(BaseEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;

                    if (entry.Properties.Any(p => p.IsModified))
                        entry.Entity.SetUpdated(_currentUserService.UserName);
                }
            }
        }

        private static string ToSnakeCaseCached(string input)
        {
            return SnakeCaseCache.GetOrAdd(input, ToSnakeCase);
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && input[i - 1] != '_')
                        builder.Append('_');

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
