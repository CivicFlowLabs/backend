using CivicFlow.Shared.Api;

namespace CivicFlow.Api.Http;

/// <summary>
/// Dựng phần mô tả lỗi từ mã trạng thái HTTP. Gom về một chỗ vì cùng lúc có
/// ba nơi cần: bộ lọc bọc phản hồi, middleware xử lý lỗi và bộ xử lý các mã
/// lỗi phát sinh trước khi request vào tới MVC.
/// </summary>
public static class ApiErrorFactory
{
    public static ApiError Create(int statusCode, string correlationId, string? message = null) => new()
    {
        Code = CodeFor(statusCode),
        Message = string.IsNullOrWhiteSpace(message) ? DefaultMessage(statusCode) : message,
        CorrelationId = correlationId
    };

    public static string CodeFor(int statusCode) => statusCode switch
    {
        400 => ErrorCodes.BadRequest,
        401 => ErrorCodes.Unauthorized,
        403 => ErrorCodes.Forbidden,
        404 => ErrorCodes.NotFound,
        405 => ErrorCodes.BadRequest,
        409 => ErrorCodes.Conflict,
        503 => ErrorCodes.ServiceUnavailable,
        _ => ErrorCodes.InternalError
    };

    public static string DefaultMessage(int statusCode) => statusCode switch
    {
        400 => "Yêu cầu không hợp lệ.",
        401 => "Bạn cần đăng nhập để thực hiện thao tác này.",
        403 => "Bạn không có quyền thực hiện thao tác này.",
        404 => "Không tìm thấy dữ liệu được yêu cầu.",
        405 => "Phương thức HTTP không được hỗ trợ cho đường dẫn này.",
        409 => "Thao tác xung đột với dữ liệu hiện có.",
        503 => "Dịch vụ tạm thời không khả dụng.",
        _ => "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau."
    };
}
