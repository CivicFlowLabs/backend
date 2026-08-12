namespace CivicFlow.Modules.AdministrativeUnits.Entities;

/// <summary>
/// Phiên bản hiện hành của bộ dữ liệu đơn vị hành chính.
/// </summary>
/// <remarks>
/// Bảng chỉ có đúng một dòng (<see cref="Id"/> luôn bằng 1, có check
/// constraint bảo đảm). Dùng làm ETag: dữ liệu hành chính gần như bất biến
/// nên có thể cache rất mạnh, và mỗi lần nạp dữ liệu mới chỉ cần đổi giá trị
/// ở đây là mọi tầng cache tự hết hiệu lực.
/// </remarks>
public sealed class DatasetVersion
{
    public int Id { get; set; }

    /// <summary>Chuỗi phiên bản, ví dụ "2025.07.01-sapnhap-r1".</summary>
    public required string Version { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public string? Note { get; set; }
}
