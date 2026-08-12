using CivicFlow.Modules.AdministrativeUnits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence.Configurations;

public sealed class AdministrativeBoundaryConfiguration
    : IEntityTypeConfiguration<AdministrativeBoundary>
{
    public void Configure(EntityTypeBuilder<AdministrativeBoundary> b)
    {
        b.ToTable("administrative_boundary");

        b.HasKey(x => x.UnitId);

        b.Property(x => x.Boundary)
            .HasColumnType("geometry(MultiPolygon,4326)")
            .IsRequired();

        b.Property(x => x.BoundaryLod1).HasColumnType("geometry(MultiPolygon,4326)");
        b.Property(x => x.BoundaryLod2).HasColumnType("geometry(MultiPolygon,4326)");

        b.Property(x => x.Centroid)
            .HasColumnType("geometry(Point,4326)")
            .IsRequired();

        b.Property(x => x.Headquarters).HasColumnType("geometry(Point,4326)");

        b.Property(x => x.Bbox)
            .HasColumnType("geometry(Polygon,4326)")
            .IsRequired();

        b.Property(x => x.AreaKm2).HasPrecision(12, 4);

        b.Property(x => x.DataSource).HasMaxLength(50).IsRequired();

        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Index GiST là điều kiện để PostGIS tự thêm bộ lọc bounding box &&
        // vào các phép point-in-polygon. Thiếu nó, ST_Contains phải quét toàn
        // bảng và không thể đạt ngưỡng 50ms của §10.
        b.HasIndex(x => x.Boundary)
            .HasDatabaseName("ix_bnd_geom")
            .HasMethod("gist");

        b.HasIndex(x => x.BoundaryLod1)
            .HasDatabaseName("ix_bnd_geom_lod1")
            .HasMethod("gist");

        b.HasIndex(x => x.Centroid)
            .HasDatabaseName("ix_bnd_centroid")
            .HasMethod("gist");
    }
}
