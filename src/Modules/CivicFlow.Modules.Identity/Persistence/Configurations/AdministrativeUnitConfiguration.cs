using CivicFlow.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CivicFlow.Modules.Identity.Persistence.Configurations;

public class AdministrativeUnitConfiguration : IEntityTypeConfiguration<AdministrativeUnit>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnit> b)
    {
        b.ToTable("administrative_unit", t =>
            t.HasCheckConstraint(
                "ck_admin_unit_type",
                "type IN ('commune', 'ward', 'special_zone')"));

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        b.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // Lưu enum thành chuỗi snake_case để khớp check constraint ở CSDL
        // và để đọc dữ liệu thô vẫn hiểu được.
        //
        // Dùng toán tử ba ngôi chứ không dùng switch: ValueConverter nhận
        // expression tree, mà expression tree không chấp nhận switch hay throw.
        b.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(new ValueConverter<AdministrativeUnitType, string>(
                v => v == AdministrativeUnitType.Commune ? "commune"
                   : v == AdministrativeUnitType.Ward ? "ward"
                   : "special_zone",
                v => v == "commune" ? AdministrativeUnitType.Commune
                   : v == "ward" ? AdministrativeUnitType.Ward
                   : AdministrativeUnitType.SpecialZone));

        b.Property(x => x.ProvinceCode)
            .HasColumnName("province_code")
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.ProvinceName)
            .HasColumnName("province_name")
            .HasMaxLength(200);

        // Kiểu không gian của PostGIS. NetTopologySuite lo phần map.
        b.Property(x => x.Boundary)
            .HasColumnName("boundary")
            .HasColumnType("geometry(MultiPolygon, 4326)");

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        b.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("uq_admin_unit_code");

        // Index GIST phục vụ phép point-in-polygon ở tuần 2 khi tự xác định
        // phản ánh thuộc đơn vị nào.
        b.HasIndex(x => x.Boundary)
            .HasMethod("GIST")
            .HasDatabaseName("ix_admin_unit_boundary");

        b.HasIndex(x => x.ProvinceCode)
            .HasDatabaseName("ix_admin_unit_province");
    }
}
