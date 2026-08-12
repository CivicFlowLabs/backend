using CivicFlow.Api.Contracts;
using CivicFlow.Modules.AdministrativeUnits.Repositories;
using CivicFlow.Modules.Identity.Persistence;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Api.Controllers;

/// <summary>
/// Kiểm tra tình trạng hệ thống. Dùng để nghiệm thu BE-03: xác nhận kết nối
/// được cơ sở dữ liệu và PostGIS đã bật.
/// </summary>
[ApiController]
[Route(ApiRoutes.Controller)]
public sealed class HealthController : ControllerBase
{
    private readonly IdentityDbContext _db;
    private readonly IAdministrativeUnitRepository _units;

    public HealthController(IdentityDbContext db, IAdministrativeUnitRepository units)
    {
        _db = db;
        _units = units;
    }

    /// <summary>Kiểm tra kết nối cơ sở dữ liệu và phiên bản PostGIS.</summary>
    [HttpGet("db")]
    [ProducesResponseType(typeof(ApiResponse<DatabaseHealthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDatabaseHealth(CancellationToken cancellationToken)
    {
        if (!await _db.Database.CanConnectAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var postgisVersion = await _db.Database
            .SqlQuery<string>($"SELECT PostGIS_Version() AS \"Value\"")
            .FirstOrDefaultAsync(cancellationToken);

        // Đơn vị hành chính do module riêng sở hữu, nên đếm qua hợp đồng công
        // khai của module đó thay vì truy cập thẳng DbContext của nó.
        var unitCount = await _units.CountCurrentAsync(cancellationToken);

        return Ok(new DatabaseHealthResponse("ok", postgisVersion, unitCount));
    }
}
