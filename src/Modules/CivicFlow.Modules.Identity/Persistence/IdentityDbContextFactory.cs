using CivicFlow.Infrastructure.Persistence;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CivicFlow.Modules.Identity.Persistence;

/// <summary>
/// Cho phép lệnh "dotnet ef" tạo DbContext lúc thiết kế mà không cần chạy
/// toàn bộ ứng dụng. Chuỗi kết nối đọc từ biến môi trường
/// CIVICFLOW_CONNECTION, nếu không có thì dùng cấu hình phát triển mặc định.
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=civicflow_dev;Username=civicflow;Password=civicflow";

    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CIVICFLOW_CONNECTION") ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseCivicFlowNpgsql(connectionString, DbSchemas.Identity)
            .Options;

        return new IdentityDbContext(options);
    }
}
