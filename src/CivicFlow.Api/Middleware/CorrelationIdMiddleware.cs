using System.Text.RegularExpressions;
using CivicFlow.Shared.Api;

namespace CivicFlow.Api.Middleware;

/// <summary>
/// Gắn mã tương quan cho mỗi request: nhận lại mã client gửi lên nếu hợp lệ,
/// nếu không thì tự sinh. Mã được trả về trong header phản hồi và đưa vào
/// phạm vi log, nhờ đó tra được toàn bộ log của một request từ một mã duy nhất.
/// </summary>
public sealed partial class CorrelationIdMiddleware
{
    private const int MaxLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[HttpConstants.CorrelationIdItemKey] = correlationId;

        // Đặt header ngay trước khi phản hồi bắt đầu gửi đi, vì lúc này
        // chưa biết middleware phía sau có ghi gì vào phản hồi hay không.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HttpConstants.CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Chỉ nhận lại mã của client khi nó đủ ngắn và chỉ gồm ký tự an toàn.
    /// Mã này đi thẳng vào log và header phản hồi nên không được tin tưởng
    /// dữ liệu thô từ bên ngoài.
    /// </summary>
    private static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HttpConstants.CorrelationIdHeader].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(incoming)
            && incoming.Length <= MaxLength
            && SafeCorrelationId().IsMatch(incoming))
        {
            return incoming;
        }

        return Guid.NewGuid().ToString("N");
    }

    [GeneratedRegex(@"^[A-Za-z0-9\-]+$")]
    private static partial Regex SafeCorrelationId();
}

/// <summary>Lấy mã tương quan của request hiện tại.</summary>
public static class CorrelationIdAccessor
{
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(HttpConstants.CorrelationIdItemKey, out var value)
               && value is string correlationId
            ? correlationId
            : string.Empty;
    }
}
