using CivicFlow.Modules.AdministrativeUnits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence.Configurations;

public sealed class AdministrativeUnitConfiguration
    : IEntityTypeConfiguration<AdministrativeUnit>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnit> b)
    {
        b.ToTable("administrative_unit", t =>
        {
            t.HasCheckConstraint("ck_adm_level", "level IN (1, 3)");
            t.HasCheckConstraint(
                "ck_adm_period",
                "valid_to IS NULL OR valid_to > valid_from");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.Code).HasMaxLength(10).IsRequired();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.ShortName).HasMaxLength(150);
        b.Property(x => x.NameNormalized).HasMaxLength(150).IsRequired();
        b.Property(x => x.UnitType).HasMaxLength(30).IsRequired();
        b.Property(x => x.ParentCode).HasMaxLength(10);
        b.Property(x => x.LegalBasis).HasMaxLength(100);
        b.Property(x => x.DataSource).HasMaxLength(50).IsRequired();

        b.Property(x => x.Level).HasConversion<short>();

        b.Property(x => x.IsAuthoritative).HasDefaultValue(false);

        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        b.HasIndex(x => x.Code).HasDatabaseName("ix_adm_code");

        b.HasIndex(x => x.ParentCode)
            .HasDatabaseName("ix_adm_parent")
            .HasFilter("valid_to IS NULL");

        b.HasIndex(x => x.Level)
            .HasDatabaseName("ix_adm_level_valid")
            .HasFilter("valid_to IS NULL");

        // Tìm kiếm mờ theo tên. Tên chỉ dùng để hiển thị và tìm kiếm — không
        // bao giờ dùng để join, vì sau sáp nhập tên xã trùng nhau rất nhiều.
        b.HasIndex(x => x.NameNormalized)
            .HasDatabaseName("ix_adm_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        // Bổ sung so với DDL §5.2. Câu upsert ở §5.3 dùng
        // ON CONFLICT (code, valid_from), mà PostgreSQL bắt buộc đích của
        // ON CONFLICT phải là một unique index — ràng buộc EXCLUDE không
        // dùng làm đích được. Về mặt ngữ nghĩa index này không nới lỏng gì:
        // hai bản ghi cùng mã và cùng ngày bắt đầu vốn đã bị EXCLUDE chặn.
        b.HasIndex(x => new { x.Code, x.ValidFrom })
            .IsUnique()
            .HasDatabaseName("uq_adm_code_valid_from");

        b.HasOne(x => x.Boundary)
            .WithOne(x => x.Unit)
            .HasForeignKey<AdministrativeBoundary>(x => x.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mặc định chỉ trả về bản ghi còn hiệu lực. Mọi truy vấn lịch sử
        // phải gọi IgnoreQueryFilters() một cách có chủ đích — xem
        // AdministrativeUnitRepository.GetAtDateAsync.
        b.HasQueryFilter(x => x.ValidTo == null);
    }
}
