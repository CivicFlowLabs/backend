using CivicFlow.Shared.Common;
using NetTopologySuite.Geometries;

namespace CivicFlow.Modules.Identity.Entities;

/// <summary>
/// Đơn vị hành chính cấp xã/phường. Đây là bảng gốc của hệ thống — mọi dữ
/// liệu nghiệp vụ đều gắn với một đơn vị để phục vụ phân quyền theo địa bàn.
/// </summary>
public class AdministrativeUnit : BaseEntity
{
    /// <summary>Mã đơn vị hành chính, duy nhất trong toàn hệ thống.</summary>
    public required string Code { get; set; }

    public required string Name { get; set; }

    public AdministrativeUnitType Type { get; set; }

    /// <summary>Mã tỉnh/thành phố quản lý đơn vị này (mô hình hai cấp).</summary>
    public required string ProvinceCode { get; set; }

    public string? ProvinceName { get; set; }

    /// <summary>
    /// Ranh giới địa lý theo hệ toạ độ WGS 84 (EPSG:4326).
    /// Dùng MultiPolygon vì nhiều xã/phường có phần đất tách rời.
    /// Cho phép null khi chưa có dữ liệu ranh giới cập nhật sau sáp nhập 2025.
    /// </summary>
    public MultiPolygon? Boundary { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
