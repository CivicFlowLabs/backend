using CivicFlow.Infrastructure.Persistence;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence;

/// <summary>
/// Cho phép lệnh "dotnet ef" tạo DbContext lúc thiết kế mà không cần chạy
/// toàn bộ ứng dụng.
/// </summary>
/// <remarks>
/// Chuỗi kết nối bắt buộc đọc từ biến môi trường và không có giá trị mặc
/// định — mặc định nghĩa là phải nhúng mật khẩu vào mã nguồn, mà mã nguồn
/// thì nằm trong kho công khai.
/// </remarks>
public class AdmDbContextFactory : IDesignTimeDbContextFactory<AdmDbContext>
{
    private const string ConnectionEnvironmentVariable = "ConnectionStrings__Default";

    public AdmDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Thiếu biến môi trường {ConnectionEnvironmentVariable}. " +
                "Hãy tạo file .env từ .env.example rồi nạp nó vào shell trước khi chạy lệnh:" +
                Environment.NewLine +
                "  set -a && source .env && set +a");
        }

        var options = new DbContextOptionsBuilder<AdmDbContext>()
            .UseCivicFlowNpgsql(connectionString, DbSchemas.Adm)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AdmDbContext(options);
    }
}
