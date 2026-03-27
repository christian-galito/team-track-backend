using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamTrack.Domain.Entities;

namespace TeamTrack.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(44)
                .IsUnicode(false);

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.Property(x => x.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.RevokedBy)
               .HasMaxLength(50)
               .IsUnicode(false);

            builder.Property(x => x.ReplacedByToken)
                .HasMaxLength(44)
                .IsUnicode(false);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45) 
                .IsUnicode(false);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(1024); 
        }
    }
}
