namespace CivicFlow.Shared.Common;

/// <summary>
/// Tên schema của từng module. Mỗi module chỉ được tạo bảng trong schema
/// của mình — đây là ranh giới để sau này tách thành dịch vụ độc lập.
/// </summary>
public static class DbSchemas
{
    /// <summary>
    /// Dữ liệu tham chiếu về đơn vị hành chính. Tách riêng khỏi schema
    /// nghiệp vụ vì đây là danh mục dùng chung, có phiên bản theo thời gian.
    /// </summary>
    public const string Adm = "adm";

    public const string Identity = "identity";
    public const string Reports = "reports";
    public const string WorkOrders = "workorders";
    public const string Knowledge = "knowledge";
    public const string Engagement = "engagement";
}
