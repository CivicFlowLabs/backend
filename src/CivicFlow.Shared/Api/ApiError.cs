using System.Text.Json.Serialization;

namespace CivicFlow.Shared.Api;

/// <summary>
/// Phần mô tả lỗi trong phong bì phản hồi. Chỉ chứa thông tin an toàn để
/// hiển thị cho client — không bao giờ đưa stack trace hay chi tiết nội bộ
/// vào đây.
/// </summary>
public sealed class ApiError
{
    /// <summary>Mã lỗi ổn định, xem <see cref="ErrorCodes"/>.</summary>
    public required string Code { get; init; }

    /// <summary>Thông báo đọc được cho người dùng.</summary>
    public required string Message { get; init; }

    /// <summary>
    /// Lỗi theo từng trường, dùng cho lỗi kiểm tra dữ liệu đầu vào.
    /// Khoá là tên trường, giá trị là danh sách thông báo của trường đó.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }

    /// <summary>
    /// Mã tương quan của request gây lỗi. Người dùng gửi mã này cho bộ phận
    /// hỗ trợ là tra được đúng log.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; init; }
}
