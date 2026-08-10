using CivicFlow.Shared.Api;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Persistence;

/// <summary>
/// Phân trang ở tầng truy vấn. Đặt ở Infrastructure vì cần EF Core, còn
/// project Shared thì cố ý không phụ thuộc vào EF Core.
/// </summary>
public static class QueryablePagingExtensions
{
    /// <summary>
    /// Chạy hai truy vấn: đếm tổng số bản ghi khớp điều kiện, rồi lấy đúng
    /// một trang. Câu truy vấn truyền vào phải được sắp xếp sẵn, vì không có
    /// ORDER BY thì PostgreSQL không bảo đảm thứ tự giữa các trang.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);

        var totalItems = await query.LongCountAsync(cancellationToken);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Pagination = new PaginationMeta
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems
            }
        };
    }
}
