using CivicFlow.Infrastructure.Persistence;
using CivicFlow.Modules.Identity.Persistence;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Modules.Identity;

/// <summary>
/// Điểm đăng ký duy nhất của module Identity. Api chỉ gọi hàm này, không
/// biết gì về cấu trúc bên trong module.
/// </summary>
public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Thiếu chuỗi kết nối 'Default' trong cấu hình.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseCivicFlowNpgsql(connectionString, DbSchemas.Identity));

        return services;
    }
}
