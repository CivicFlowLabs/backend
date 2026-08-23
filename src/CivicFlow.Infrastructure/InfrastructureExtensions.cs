using CivicFlow.Infrastructure.Auth;
using CivicFlow.Shared.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Infrastructure;

/// <summary>
/// Điểm đăng ký các dịch vụ hạ tầng dùng chung (Infrastructure).
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
