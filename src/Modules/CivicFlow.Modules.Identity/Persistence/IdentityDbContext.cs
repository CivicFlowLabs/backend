using CivicFlow.Modules.Identity.Entities;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.Identity.Persistence;

/// <summary>
/// DbContext riêng của module Identity. Mỗi module có DbContext và schema
/// riêng để giữ ranh giới — module khác không được truy cập context này.
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DbSchemas.Identity);

        // PostGIS cần cho cột boundary. Khai báo ở đây để migration tự sinh
        // lệnh CREATE EXTENSION.
        modelBuilder.HasPostgresExtension("postgis");

        // Nạp toàn bộ IEntityTypeConfiguration trong assembly của module này.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
