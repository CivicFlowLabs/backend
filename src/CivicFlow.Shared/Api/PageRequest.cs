namespace CivicFlow.Shared.Api;

/// <summary>
/// Tham số phân trang nhận từ query string, ví dụ "?page=2&amp;pageSize=50".
/// Giá trị được kẹp ngay trong setter nên tầng truy vấn luôn nhận được số
/// hợp lệ, kể cả khi client gửi số âm hoặc quá lớn.
/// </summary>
public class PageRequest
{
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Trần cứng cho kích thước trang. Có trần này thì không endpoint nào
    /// trả về danh sách không giới hạn, dù client yêu cầu bao nhiêu.
    /// </summary>
    public const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Số bản ghi cần bỏ qua, dùng cho câu truy vấn.</summary>
    public int Skip => (Page - 1) * PageSize;
}
