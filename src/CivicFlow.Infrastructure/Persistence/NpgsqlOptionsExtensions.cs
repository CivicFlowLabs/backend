using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Persistence;

/// <summary>
/// Cấu hình Npgsql dùng chung cho mọi module: bật NetTopologySuite để
/// map kiểu dữ liệu không gian của PostGIS (Point, MultiPolygon...),
/// và chỉ định bảng lịch sử migration nằm trong schema riêng của module.
/// </summary>
public static class NpgsqlOptionsExtensions
{
    /// <summary>
    /// Dùng trong lambda của AddDbContext, nơi nhận DbContextOptionsBuilder
    /// không generic.
    /// </summary>
    public static DbContextOptionsBuilder UseCivicFlowNpgsql(
        this DbContextOptionsBuilder builder,
        string connectionString,
        string schema)
    {
        return builder.UseNpgsql(connectionString, npgsql =>
        {
            // Bắt buộc để EF hiểu kiểu geometry của PostGIS.
            npgsql.UseNetTopologySuite();

            // Mỗi module có bảng lịch sử migration riêng trong schema của mình,
            // nhờ đó migration của các module không giẫm chân nhau.
            npgsql.MigrationsHistoryTable("__ef_migrations_history", schema);
        });
    }

    /// <summary>
    /// Dùng trong IDesignTimeDbContextFactory, nơi cần giữ kiểu generic để
    /// lấy ra DbContextOptions&lt;TContext&gt; truyền vào constructor.
    /// </summary>
    public static DbContextOptionsBuilder<TContext> UseCivicFlowNpgsql<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string connectionString,
        string schema)
        where TContext : DbContext
    {
        ((DbContextOptionsBuilder)builder).UseCivicFlowNpgsql(connectionString, schema);
        return builder;
    }
}
