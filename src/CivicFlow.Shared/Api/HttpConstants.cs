namespace CivicFlow.Shared.Api;

/// <summary>Hằng số dùng chung ở tầng HTTP.</summary>
public static class HttpConstants
{
    /// <summary>
    /// Tên header mang mã tương quan. Client có thể tự gửi lên để nối một
    /// thao tác của người dùng với log ở phía máy chủ; nếu không gửi thì
    /// máy chủ tự sinh.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>Khoá lưu mã tương quan trong HttpContext.Items.</summary>
    public const string CorrelationIdItemKey = "CivicFlow:CorrelationId";
}
