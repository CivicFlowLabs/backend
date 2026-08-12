using CivicFlow.Modules.AdministrativeUnits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence.Configurations;

public sealed class UnitMappingConfiguration : IEntityTypeConfiguration<UnitMapping>
{
    public void Configure(EntityTypeBuilder<UnitMapping> b)
    {
        b.ToTable("unit_mapping");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).UseIdentityAlwaysColumn();

        b.Property(x => x.OldCode).HasMaxLength(10).IsRequired();
        b.Property(x => x.OldFullName).HasMaxLength(300).IsRequired();
        b.Property(x => x.OldNameNorm).HasMaxLength(300).IsRequired();
        b.Property(x => x.NewCode).HasMaxLength(10);
        b.Property(x => x.MappingType).HasMaxLength(20).IsRequired();
        b.Property(x => x.LegalBasis).HasMaxLength(100).IsRequired();

        b.Property(x => x.Confidence)
            .HasPrecision(4, 3)
            .HasDefaultValue(1.000m);

        b.HasIndex(x => new { x.OldCode, x.OldLevel }).HasDatabaseName("ix_map_old");
        b.HasIndex(x => x.NewCode).HasDatabaseName("ix_map_new");

        // Tầng 3 của thuật toán resolve (§8.2) khớp mờ trên cột này.
        b.HasIndex(x => x.OldNameNorm)
            .HasDatabaseName("ix_map_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
