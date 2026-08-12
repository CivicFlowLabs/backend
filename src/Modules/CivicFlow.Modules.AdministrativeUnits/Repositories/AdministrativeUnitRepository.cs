using CivicFlow.Modules.AdministrativeUnits.Entities;
using CivicFlow.Modules.AdministrativeUnits.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.AdministrativeUnits.Repositories;

/// <inheritdoc />
public sealed class AdministrativeUnitRepository(AdmDbContext db)
    : IAdministrativeUnitRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<AdministrativeUnit>> GetCurrentAsync(
        AdmLevel? level = null,
        CancellationToken ct = default)
    {
        // Không bỏ bộ lọc toàn cục: valid_to IS NULL chính là định nghĩa
        // của "đang còn hiệu lực".
        var query = db.AdministrativeUnits.AsNoTracking();

        if (level is { } lv)
        {
            query = query.Where(u => u.Level == lv);
        }

        return await query.OrderBy(u => u.Code).ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdministrativeUnit>> GetAtDateAsync(
        DateOnly at,
        AdmLevel? level = null,
        CancellationToken ct = default)
    {
        // BẮT BUỘC bỏ bộ lọc toàn cục. Bản ghi có hiệu lực trong quá khứ đã
        // được đóng lại (valid_to khác null), nên nếu giữ bộ lọc thì truy vấn
        // này luôn trả về rỗng đối với mọi đơn vị đã thay đổi.
        var query = db.AdministrativeUnits
            .IgnoreQueryFilters()
            .AsNoTracking()
            // Khoảng hiệu lực nửa mở [valid_from, valid_to), khớp đúng với
            // daterange '[)' mà ràng buộc EXCLUDE dùng để chặn chồng lấn.
            .Where(u => u.ValidFrom <= at && (u.ValidTo == null || u.ValidTo > at));

        if (level is { } lv)
        {
            query = query.Where(u => u.Level == lv);
        }

        return await query.OrderBy(u => u.Code).ToListAsync(ct);
    }
}
