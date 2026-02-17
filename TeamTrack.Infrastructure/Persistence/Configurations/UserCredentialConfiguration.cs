using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Persistence.Configurations
{
    public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
    {
        public void Configure(EntityTypeBuilder<UserCredential> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.HashedPassword)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(x => x.UserId);
        }
    }
}
