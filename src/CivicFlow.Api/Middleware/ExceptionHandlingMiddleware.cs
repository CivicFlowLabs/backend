using CivicFlow.Shared.Api;
using CivicFlow.Shared.Exceptions;
using FluentValidation;

namespace CivicFlow.Api.Middleware;

/// <summary>
/// Lưới an toàn cuối cùng: bắt mọi ngoại lệ chưa được xử lý và trả về đúng
/// phong bì phản hồi chung, để client không bao giờ nhận được trang lỗi HTML
/// hay phần thân rỗng.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client đóng kết nối giữa chừng. Không phải lỗi của máy chủ và
            // cũng không còn ai nhận phản hồi, nên chỉ ghi nhận ở mức thấp.
            _logger.LogInformation(
                "Client huỷ request {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                // Đã gửi header đi rồi thì không sửa được phản hồi nữa;
                // ghi log rồi để tầng máy chủ ngắt kết nối.
                _logger.LogError(
                    exception,
                    "Lỗi xảy ra sau khi phản hồi đã bắt đầu gửi: {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();
        var (statusCode, error) = Translate(exception, correlationId);

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Lỗi chưa xử lý khi thực hiện {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Request {Method} {Path} thất bại với mã {ErrorCode}: {Reason}",
                context.Request.Method,
                context.Request.Path,
                error.Code,
                exception.Message);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.Headers[HttpConstants.CorrelationIdHeader] = correlationId;

        await context.Response.WriteAsJsonAsync(ApiResponse.Failure(error), context.RequestAborted);
    }

    private (int StatusCode, ApiError Error) Translate(Exception exception, string correlationId)
    {
        switch (exception)
        {
            case ValidationException validationException:
                var details = validationException.Errors
                    .GroupBy(failure => failure.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(failure => failure.ErrorMessage).ToArray());

                return (400, new ApiError
                {
                    Code = ErrorCodes.ValidationError,
                    Message = "Dữ liệu gửi lên không hợp lệ.",
                    Details = details,
                    CorrelationId = correlationId
                });

            case AppException appException:
                return (appException.StatusCode, new ApiError
                {
                    Code = appException.Code,
                    Message = appException.Message,
                    CorrelationId = correlationId
                });

            default:
                // Ngoại lệ ngoài dự kiến có thể chứa thông tin nội bộ, nên chỉ
                // lộ chi tiết khi đang phát triển.
                return (500, new ApiError
                {
                    Code = ErrorCodes.InternalError,
                    Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
                    CorrelationId = correlationId
                });
        }
    }
}
