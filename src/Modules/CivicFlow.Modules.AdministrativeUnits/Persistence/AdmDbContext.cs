using CivicFlow.Modules.AdministrativeUnits.Entities;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence;

/// <summary>
/// DbContext riêng của module Đơn vị hành chính. Module khác không được
/// truy cập context này — nếu cần dữ liệu hành chính thì gọi qua hợp đồng
/// công khai của module.
/// </summary>
public class AdmDbContext : DbContext
{
    public AdmDbContext(DbContextOptions<AdmDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// ⚠️ CÓ BỘ LỌC TOÀN CỤC: DbSet này chỉ trả về bản ghi còn hiệu lực
    /// (<c>valid_to IS NULL</c>).
    /// </summary>
    /// <remarks>
    /// Đây là chỗ dễ sinh lỗi âm thầm nhất của module. Mọi truy vấn dữ liệu
    /// lịch sử — báo cáo so sánh theo năm, tra cứu hồ sơ lập trước
    /// 01/07/2025 — sẽ trả về rỗng hoặc thiếu nếu quên bỏ bộ lọc, mà không
    /// báo lỗi gì cả.
    /// <para>
    /// Đừng gọi <c>IgnoreQueryFilters()</c> rải rác trong mã nghiệp vụ. Dùng
    /// <c>IAdministrativeUnitRepository.GetAtDateAsync</c>, nơi việc bỏ lọc
    /// đã được xử lý đúng và có kèm điều kiện khoảng thời gian.
    /// </para>
    /// </remarks>
    public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();

    public DbSet<AdministrativeBoundary> AdministrativeBoundaries =>
        Set<AdministrativeBoundary>();

    public DbSet<UnitMapping> UnitMappings => Set<UnitMapping>();

    public DbSet<DatasetVersion> DatasetVersions => Set<DatasetVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DbSchemas.Adm);

        // Khai báo ở đây để migration tự sinh lệnh CREATE EXTENSION, không
        // bắt mỗi lập trình viên phải chạy tay khi dựng máy mới.
        modelBuilder
            .HasPostgresExtension("postgis")      // kiểu và hàm không gian
            .HasPostgresExtension("pg_trgm")      // tìm kiếm mờ theo tên
            .HasPostgresExtension("unaccent")     // bỏ dấu tiếng Việt
            .HasPostgresExtension("btree_gist");  // cần cho ràng buộc EXCLUDE

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdmDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
