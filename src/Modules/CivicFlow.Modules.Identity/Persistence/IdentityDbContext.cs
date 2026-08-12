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

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DbSchemas.Identity);

        // Module này không còn cột không gian nào, nhưng vẫn phải giữ khai
        // báo. Extension thuộc phạm vi cả cơ sở dữ liệu chứ không riêng
        // schema: bỏ dòng này đi thì migration kế tiếp của Identity sẽ sinh
        // lệnh DROP EXTENSION, kéo theo mọi cột geometry của schema adm.
        // Khai báo trùng ở nhiều module là vô hại vì lệnh sinh ra có dạng
        // CREATE EXTENSION IF NOT EXISTS.
        modelBuilder.HasPostgresExtension("postgis");

        // Nạp toàn bộ IEntityTypeConfiguration trong assembly của module này.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
