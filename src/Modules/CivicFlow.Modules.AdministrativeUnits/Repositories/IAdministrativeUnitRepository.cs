using CivicFlow.Modules.AdministrativeUnits.Entities;

namespace CivicFlow.Modules.AdministrativeUnits.Repositories;

/// <summary>
/// Truy cập danh mục đơn vị hành chính.
/// </summary>
/// <remarks>
/// Hai phương thức đọc được tách bạch có chủ đích: một cho hiện tại, một cho
/// quá khứ. Việc tách này để lập trình viên phải nói rõ mình muốn mốc thời
/// gian nào, thay vì vô tình nhận dữ liệu hiện hành khi đang xử lý hồ sơ cũ.
/// </remarks>
public interface IAdministrativeUnitRepository
{
    /// <summary>
    /// Lấy các đơn vị đang còn hiệu lực tại thời điểm hiện tại.
    /// </summary>
    /// <param name="level">Lọc theo cấp; bỏ trống để lấy mọi cấp.</param>
    /// <param name="ct">Thẻ huỷ tác vụ.</param>
    Task<IReadOnlyList<AdministrativeUnit>> GetCurrentAsync(
        AdmLevel? level = null,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy các đơn vị có hiệu lực tại một ngày trong quá khứ.
    /// </summary>
    /// <param name="at">
    /// Ngày cần tra. Ví dụ 2024-01-01 sẽ trả về cấu trúc 63 tỉnh chứ không
    /// phải 34 tỉnh như hiện nay.
    /// </param>
    /// <param name="level">Lọc theo cấp; bỏ trống để lấy mọi cấp.</param>
    /// <param name="ct">Thẻ huỷ tác vụ.</param>
    Task<IReadOnlyList<AdministrativeUnit>> GetAtDateAsync(
        DateOnly at,
        AdmLevel? level = null,
        CancellationToken ct = default);

    /// <summary>
    /// Đếm số đơn vị đang còn hiệu lực, không kéo bản ghi về bộ nhớ.
    /// </summary>
    /// <param name="ct">Thẻ huỷ tác vụ.</param>
    Task<int> CountCurrentAsync(CancellationToken ct = default);
}
