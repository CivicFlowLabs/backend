namespace CivicFlow.Shared.Api;

/// <summary>
/// Thông tin phân trang kèm theo mỗi danh sách. Có đủ dữ liệu để client
/// dựng thanh phân trang mà không phải tự tính.
/// </summary>
public sealed class PaginationMeta
{
    /// <summary>Trang hiện tại, đánh số từ 1.</summary>
    public required int Page { get; init; }

    public required int PageSize { get; init; }

    /// <summary>
    /// Tổng số bản ghi khớp điều kiện lọc. Dùng long vì bảng phản ánh có thể
    /// vượt ngưỡng int khi hệ thống chạy nhiều năm.
    /// </summary>
    public required long TotalItems { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}
