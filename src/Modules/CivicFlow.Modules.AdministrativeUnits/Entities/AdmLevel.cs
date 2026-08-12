namespace CivicFlow.Modules.AdministrativeUnits.Entities;

/// <summary>
/// Cấp đơn vị hành chính theo mô hình hai cấp có hiệu lực từ 01/07/2025
/// (Nghị quyết 202/2025/QH15). Cấp huyện đã bị bỏ hoàn toàn nên không có
/// giá trị 2 — khoảng trống này là cố ý, giữ lại để mã cấp không bị dịch
/// so với dữ liệu lịch sử trước 2025.
/// </summary>
public enum AdmLevel : short
{
    /// <summary>Tỉnh hoặc thành phố trực thuộc Trung ương.</summary>
    Province = 1,

    /// <summary>Xã, phường hoặc đặc khu.</summary>
    Ward = 3
}
