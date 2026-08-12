using CivicFlow.Modules.AdministrativeUnits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence.Configurations;

public sealed class DatasetVersionConfiguration : IEntityTypeConfiguration<DatasetVersion>
{
    public void Configure(EntityTypeBuilder<DatasetVersion> b)
    {
        b.ToTable("dataset_version", t =>
            t.HasCheckConstraint("ck_singleton", "id = 1"));

        b.HasKey(x => x.Id);

        // Không sinh tự động: bảng chỉ có đúng một dòng với id = 1.
        b.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasDefaultValue(1);

        b.Property(x => x.Version).HasMaxLength(40).IsRequired();

        b.Property(x => x.PublishedAt).HasDefaultValueSql("now()");
    }
}
