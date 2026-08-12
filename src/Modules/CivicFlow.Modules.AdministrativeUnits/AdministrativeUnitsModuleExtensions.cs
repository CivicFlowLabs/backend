using CivicFlow.Infrastructure.Persistence;
using CivicFlow.Modules.AdministrativeUnits.Persistence;
using CivicFlow.Modules.AdministrativeUnits.Repositories;
using CivicFlow.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Modules.AdministrativeUnits;

/// <summary>
/// Điểm đăng ký duy nhất của module Đơn vị hành chính. Api chỉ gọi hàm này,
/// không biết gì về cấu trúc bên trong module.
/// </summary>
public static class AdministrativeUnitsModuleExtensions
{
    public static IServiceCollection AddAdministrativeUnitsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Missing connection string default in configuration");

        services.AddDbContext<AdmDbContext>(options => options
            .UseCivicFlowNpgsql(connectionString, DbSchemas.Adm)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IAdministrativeUnitRepository, AdministrativeUnitRepository>();

        return services;
    }
}
