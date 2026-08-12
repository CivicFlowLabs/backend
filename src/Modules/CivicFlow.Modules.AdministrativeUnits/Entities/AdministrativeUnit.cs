namespace CivicFlow.Modules.AdministrativeUnits.Entities;

/// <summary>
/// Một phiên bản của đơn vị hành chính trong một khoảng thời gian hiệu lực.
/// </summary>
/// <remarks>
/// Đây là bảng có phiên bản theo thời gian, không phải bảng trạng thái hiện
/// tại. Một mã đơn vị có thể có nhiều bản ghi, mỗi bản ghi ứng với một
/// khoảng <see cref="ValidFrom"/> đến <see cref="ValidTo"/> — vì vậy
/// <see cref="Code"/> cố ý KHÔNG duy nhất. Ràng buộc EXCLUDE ở tầng CSDL
/// bảo đảm hai bản ghi cùng mã không bao giờ chồng lấn thời gian.
/// </remarks>
public sealed class AdministrativeUnit
{
    public Guid Id { get; set; }

    /// <summary>
    /// Mã đơn vị hành chính theo chuẩn Cục Thống kê. Đây là khoá dùng cho
    /// mọi phép join giữa dữ liệu hành chính — không bao giờ join bằng tên,
    /// vì sau sáp nhập 2025 tên xã trùng lặp rất nhiều trên toàn quốc.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>Tên đầy đủ, ví dụ "Phường Hòa Bình".</summary>
    public required string Name { get; set; }

    /// <summary>Tên rút gọn, ví dụ "Hòa Bình".</summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// Tên đã chuẩn hoá về chữ thường và bỏ dấu, phục vụ tìm kiếm mờ bằng
    /// pg_trgm. Sinh lúc nạp dữ liệu, không nhập tay.
    /// </summary>
    public required string NameNormalized { get; set; }

    public AdmLevel Level { get; set; }

    /// <summary>
    /// Loại đơn vị: tinh, thanh_pho_tw, xa, phuong, dac_khu. Lưu dạng chuỗi
    /// snake_case kèm check constraint ở CSDL.
    /// </summary>
    public required string UnitType { get; set; }

    /// <summary>Mã tỉnh quản lý đơn vị này; null với chính cấp tỉnh.</summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// Ngày bắt đầu hiệu lực. Dùng <see cref="DateOnly"/> ánh xạ sang kiểu
    /// date của PostgreSQL — tránh hoàn toàn vấn đề DateTimeKind của Npgsql.
    /// </summary>
    public DateOnly ValidFrom { get; set; }

    /// <summary>Ngày hết hiệu lực; null nghĩa là bản ghi còn hiệu lực.</summary>
    public DateOnly? ValidTo { get; set; }

    /// <summary>Căn cứ pháp lý, ví dụ "Nghị quyết số 1668/NQ-UBTVQH15".</summary>
    public string? LegalBasis { get; set; }

    /// <summary>Nguồn dữ liệu, ví dụ SAPNHAP_BANDO, GSO, VNSDI.</summary>
    public required string DataSource { get; set; }

    /// <summary>
    /// Chỉ đặt true khi đã có văn bản xác nhận quyền sử dụng và tái phân
    /// phối từ cơ quan chủ quản nguồn dữ liệu.
    /// </summary>
    public bool IsAuthoritative { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AdministrativeBoundary? Boundary { get; set; }
}
