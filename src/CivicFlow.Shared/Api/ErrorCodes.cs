namespace CivicFlow.Shared.Api;

/// <summary>
/// Mã lỗi trả về cho client. Dùng snake_case giống quy ước enum của dự án.
/// Client nên bắt theo mã này thay vì so khớp chuỗi thông báo, vì thông báo
/// có thể đổi hoặc được dịch.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "validation_error";
    public const string BadRequest = "bad_request";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string ServiceUnavailable = "service_unavailable";
    public const string InternalError = "internal_error";
}
