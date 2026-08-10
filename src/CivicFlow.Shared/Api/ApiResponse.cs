using System.Text.Json.Serialization;

namespace CivicFlow.Shared.Api;

/// <summary>
/// Phong bì phản hồi dùng chung cho mọi endpoint.
/// </summary>
/// <remarks>
/// Quy ước tuần tự hoá:
/// <list type="bullet">
/// <item><c>data</c> luôn xuất hiện, bằng null khi có lỗi.</item>
/// <item><c>error</c> chỉ xuất hiện khi thất bại.</item>
/// <item><c>pagination</c> chỉ xuất hiện với endpoint trả về danh sách.</item>
/// </list>
/// Nhờ vậy client chỉ cần kiểm tra sự tồn tại của <c>error</c> là biết
/// request thành công hay không.
/// </remarks>
public sealed class ApiResponse<T>
{
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Pagination { get; init; }
}

/// <summary>Các hàm dựng phong bì phản hồi.</summary>
public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data) => new() { Data = data };

    public static ApiResponse<T> Success<T>(T data, PaginationMeta pagination) =>
        new() { Data = data, Pagination = pagination };

    public static ApiResponse<object> Failure(ApiError error) => new() { Error = error };
}
