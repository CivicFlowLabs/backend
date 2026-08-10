using CivicFlow.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.Identity.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_token");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        b.Property(x => x.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        b.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        b.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        b.Property(x => x.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(45);

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // IsActive là thuộc tính tính toán trong C#, không lưu xuống CSDL.
        b.Ignore(x => x.IsActive);

        b.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_refresh_token_user")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("uq_refresh_token");

        b.HasIndex(x => x.UserId)
            .HasFilter("revoked_at IS NULL")
            .HasDatabaseName("ix_refresh_token_user");

        b.HasIndex(x => x.ExpiresAt)
            .HasFilter("revoked_at IS NULL")
            .HasDatabaseName("ix_refresh_token_expires");
    }
}
