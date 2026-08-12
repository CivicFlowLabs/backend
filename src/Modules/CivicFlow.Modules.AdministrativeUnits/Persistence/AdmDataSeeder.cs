using CivicFlow.Modules.AdministrativeUnits.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.AdministrativeUnits.Persistence;

/// <summary>
/// Nạp vài đơn vị hành chính mồi để chạy được ở máy phát triển.
/// </summary>
/// <remarks>
/// Đây là dữ liệu tạm, không phải danh mục thật. Dữ liệu chính thức gồm 34
/// tỉnh và 3.323 xã sẽ do công cụ crawl và nạp từ nguồn của Nhà xuất bản Tài
/// nguyên - Môi trường và Bản đồ Việt Nam đưa vào. Vì vậy mọi bản ghi ở đây
/// đều mang <c>data_source = "SEED"</c> và <c>is_authoritative = false</c> để
/// phân biệt được với dữ liệu thật khi đối soát.
/// </remarks>
public static class AdmDataSeeder
{
    /// <summary>
    /// Mã đơn vị mặc định dùng cho tài khoản mẫu. Là mã chứ không phải khoá
    /// chính, vì mọi phép tra cứu giữa dữ liệu hành chính đều đi qua mã.
    /// </summary>
    public const string DefaultUnitCode = "76001";

    /// <summary>Ngày Nghị quyết 202/2025/QH15 có hiệu lực.</summary>
    private static readonly DateOnly EffectiveFrom = new(2025, 7, 1);

    /// <summary>
    /// Nạp dữ liệu mồi nếu bảng đang trống, rồi trả về khoá chính của đơn vị
    /// mặc định để module khác gán cho bản ghi của mình.
    /// </summary>
    /// <param name="context">Context của module Đơn vị hành chính.</param>
    /// <param name="ct">Thẻ huỷ tác vụ.</param>
    /// <returns>Khoá chính của đơn vị có mã <see cref="DefaultUnitCode"/>.</returns>
    public static async Task<Guid> SeedAsync(AdmDbContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!await context.AdministrativeUnits.AnyAsync(ct))
        {
            context.AdministrativeUnits.AddRange(
                NewUnit(
                    id: Guid.Parse("79000000-0000-0000-0000-000000000079"),
                    code: "79",
                    name: "Thành phố Hồ Chí Minh",
                    nameNormalized: "ho chi minh",
                    level: AdmLevel.Province,
                    unitType: "thanh_pho_tw",
                    parentCode: null),
                NewUnit(
                    id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    code: DefaultUnitCode,
                    name: "Phường Bến Nghé",
                    nameNormalized: "ben nghe",
                    level: AdmLevel.Ward,
                    unitType: "phuong",
                    parentCode: "79"),
                NewUnit(
                    id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    code: "76002",
                    name: "Phường Bến Thành",
                    nameNormalized: "ben thanh",
                    level: AdmLevel.Ward,
                    unitType: "phuong",
                    parentCode: "79"));

            await context.SaveChangesAsync(ct);
        }

        return await context.AdministrativeUnits
            .Where(u => u.Code == DefaultUnitCode)
            .Select(u => u.Id)
            .FirstAsync(ct);
    }

    private static AdministrativeUnit NewUnit(
        Guid id,
        string code,
        string name,
        string nameNormalized,
        AdmLevel level,
        string unitType,
        string? parentCode) => new()
        {
            Id = id,
            Code = code,
            Name = name,
            NameNormalized = nameNormalized,
            Level = level,
            UnitType = unitType,
            ParentCode = parentCode,
            ValidFrom = EffectiveFrom,
            ValidTo = null,
            LegalBasis = "NQ 202/2025/QH15",
            DataSource = "SEED",
            IsAuthoritative = false
        };
}
