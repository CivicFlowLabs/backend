using CivicFlow.Shared.Api;

namespace CivicFlow.Shared.Exceptions;

/// <summary>
/// Lỗi nghiệp vụ đã lường trước, có mã và mã trạng thái HTTP đi kèm.
/// Middleware xử lý lỗi dựa vào hai thông tin này để dựng phản hồi, nhờ đó
/// tầng nghiệp vụ không cần biết gì về HTTP.
/// </summary>
/// <remarks>
/// Mã trạng thái để kiểu int thay vì dùng StatusCodes của ASP.NET Core, để
/// project Shared không phải phụ thuộc vào ASP.NET Core.
/// </remarks>
public abstract class AppException : Exception
{
    protected AppException(string code, string message, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }
}

/// <summary>Không tìm thấy tài nguyên được yêu cầu.</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(ErrorCodes.NotFound, message, 404)
    {
    }
}

/// <summary>
/// Thao tác xung đột với trạng thái hiện tại, ví dụ trùng mã đơn vị hành
/// chính hoặc chuyển trạng thái không hợp lệ.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(ErrorCodes.Conflict, message, 409)
    {
    }
}

/// <summary>
/// Đã đăng nhập nhưng không đủ quyền — thường là truy cập dữ liệu ngoài
/// địa bàn được phân công.
/// </summary>
public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(ErrorCodes.Forbidden, message, 403)
    {
    }
}

/// <summary>Dữ liệu đầu vào không hợp lệ theo quy tắc nghiệp vụ.</summary>
public sealed class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(ErrorCodes.BadRequest, message, 400)
    {
    }
}

/// <summary>Chưa đăng nhập hoặc thông tin đăng nhập không chính xác.</summary>
public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message)
        : base(ErrorCodes.Unauthorized, message, 401)
    {
    }
}
