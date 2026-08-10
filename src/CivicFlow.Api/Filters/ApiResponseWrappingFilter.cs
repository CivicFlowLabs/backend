using CivicFlow.Api.Http;
using CivicFlow.Api.Middleware;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CivicFlow.Api.Filters;

/// <summary>
/// Bọc kết quả của mọi action vào phong bì phản hồi chung, để controller chỉ
/// việc trả về DTO thuần và không phải lặp lại việc dựng phong bì.
/// </summary>
/// <remarks>
/// Bốn trường hợp được xử lý:
/// <list type="bullet">
/// <item><see cref="PagedResult{T}"/> tách thành <c>data</c> và <c>pagination</c>.</item>
/// <item>Giá trị đã là <see cref="ApiResponse{T}"/> thì giữ nguyên, tránh bọc hai lần.</item>
/// <item>Kết quả không có thân nhưng mã trạng thái từ 400 trở lên được đổi thành phong bì lỗi.</item>
/// <item>Các giá trị còn lại đưa vào <c>data</c>.</item>
/// </list>
/// Phản hồi 204 giữ nguyên vì theo chuẩn HTTP thì 204 không được có thân.
/// </remarks>
public sealed class ApiResponseWrappingFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var correlationId = context.HttpContext.GetCorrelationId();

        switch (context.Result)
        {
            case ObjectResult objectResult when !IsAlreadyWrapped(objectResult.Value):
                objectResult.Value = Wrap(objectResult.Value, objectResult.StatusCode, correlationId);
                objectResult.DeclaredType = null;
                break;

            case StatusCodeResult statusCodeResult when statusCodeResult.StatusCode >= 400:
                context.Result = new ObjectResult(
                    ApiResponse.Failure(ApiErrorFactory.Create(statusCodeResult.StatusCode, correlationId)))
                {
                    StatusCode = statusCodeResult.StatusCode
                };
                break;
        }

        return next();
    }

    private static object Wrap(object? value, int? statusCode, string correlationId)
    {
        if (statusCode >= 400)
        {
            // ProblemDetails do ASP.NET Core tự sinh ở một số nhánh; đổi sang
            // phong bì chung để client chỉ phải xử lý một định dạng.
            var message = value is ProblemDetails problem ? problem.Detail : null;

            return ApiResponse.Failure(ApiErrorFactory.Create(statusCode.Value, correlationId, message));
        }

        return value is IPagedResult paged
            ? ApiResponse.Success<object?>(paged.Items, paged.Pagination)
            : ApiResponse.Success<object?>(value);
    }

    private static bool IsAlreadyWrapped(object? value) =>
        value is not null
        && value.GetType() is { IsGenericType: true } type
        && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
}
