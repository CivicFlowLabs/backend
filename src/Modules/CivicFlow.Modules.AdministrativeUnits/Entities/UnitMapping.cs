namespace CivicFlow.Modules.AdministrativeUnits.Entities;

/// <summary>
/// Ánh xạ một đơn vị hành chính cũ sang đơn vị mới sau sắp xếp.
/// </summary>
/// <remarks>
/// Với trường hợp <c>split</c> — một xã cũ tách vào nhiều xã mới — sẽ có
/// nhiều dòng cùng <see cref="OldCode"/>. Khi đó API phải trả toàn bộ ứng
/// viên cho cán bộ chọn, tuyệt đối không tự chọn một cái.
/// </remarks>
public sealed class UnitMapping
{
    public long Id { get; set; }

    public required string OldCode { get; set; }

    /// <summary>
    /// Cấp của đơn vị cũ. Nhận cả giá trị 2 (huyện) vì dữ liệu trước
    /// 01/07/2025 vẫn còn cấp huyện, dù mô hình hiện tại đã bỏ cấp này.
    /// </summary>
    public short OldLevel { get; set; }

    /// <summary>
    /// Địa chỉ đầy đủ của đơn vị cũ, ví dụ
    /// "Xã An Bình, Huyện Long Hồ, Tỉnh Vĩnh Long".
    /// </summary>
    public required string OldFullName { get; set; }

    /// <summary>Bản chuẩn hoá của <see cref="OldFullName"/> cho tìm kiếm mờ.</summary>
    public required string OldNameNorm { get; set; }

    /// <summary>Mã đơn vị mới; null khi đơn vị bị giải thể không có kế thừa.</summary>
    public string? NewCode { get; set; }

    public short? NewLevel { get; set; }

    /// <summary>
    /// unchanged, renamed, merged, split, transferred hoặc dissolved.
    /// </summary>
    public required string MappingType { get; set; }

    /// <summary>
    /// Độ tin cậy của ánh xạ, từ 0 đến 1. Giá trị dưới 1 nghĩa là ánh xạ được
    /// suy luận chứ không lấy trực tiếp từ văn bản pháp lý — theo §8.3 những
    /// trường hợp dưới 0.9 phải vào hàng đợi cán bộ duyệt thủ công.
    /// </summary>
    public decimal Confidence { get; set; }

    public string? Note { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public required string LegalBasis { get; set; }
}
