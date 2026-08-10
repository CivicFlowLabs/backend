using CivicFlow.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.Identity.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("app_user", t =>
        {
            t.HasCheckConstraint(
                "ck_app_user_role",
                "role IN ('citizen', 'officer', 'leader', 'admin')");

            // Phải có ít nhất một cách liên lạc thì mới đăng nhập được.
            t.HasCheckConstraint(
                "ck_app_user_contact",
                "email IS NOT NULL OR phone_number IS NOT NULL");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

        b.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        b.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        b.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<UserRole>(v, ignoreCase: true));

        b.Property(x => x.AdministrativeUnitId)
            .HasColumnName("administrative_unit_id")
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        b.HasOne(x => x.AdministrativeUnit)
            .WithMany(u => u.Users)
            .HasForeignKey(x => x.AdministrativeUnitId)
            .HasConstraintName("fk_app_user_admin_unit")
            .OnDelete(DeleteBehavior.Restrict);

        // Index unique một phần: chỉ áp dụng khi cột có giá trị, nhờ đó nhiều
        // bản ghi cùng để null vẫn hợp lệ.
        b.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("email IS NOT NULL")
            .HasDatabaseName("uq_app_user_email");

        b.HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("phone_number IS NOT NULL")
            .HasDatabaseName("uq_app_user_phone");

        // Truy vấn phổ biến nhất của hệ thống: lọc theo địa bàn và vai trò.
        b.HasIndex(x => new { x.AdministrativeUnitId, x.Role })
            .HasFilter("is_active")
            .HasDatabaseName("ix_app_user_unit_role");
    }
}
