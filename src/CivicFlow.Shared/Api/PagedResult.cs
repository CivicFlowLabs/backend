using System.Collections;

namespace CivicFlow.Shared.Api;

/// <summary>
/// Mặt không generic của <see cref="PagedResult{T}"/>. Nhờ nó mà bộ lọc bọc
/// phản hồi tách được phần dữ liệu và phần phân trang mà không cần reflection.
/// </summary>
public interface IPagedResult
{
    IEnumerable Items { get; }

    PaginationMeta Pagination { get; }
}

/// <summary>
/// Kết quả một trang dữ liệu. Tầng nghiệp vụ trả về kiểu này, còn việc dựng
/// phong bì phản hồi do tầng API lo.
/// </summary>
public sealed class PagedResult<T> : IPagedResult
{
    public required IReadOnlyList<T> Items { get; init; }

    public required PaginationMeta Pagination { get; init; }

    // Cài đặt tường minh để không đụng tên với thuộc tính Items ở trên.
    IEnumerable IPagedResult.Items => Items;
}
