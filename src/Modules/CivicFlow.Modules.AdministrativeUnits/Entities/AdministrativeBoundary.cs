using NetTopologySuite.Geometries;

namespace CivicFlow.Modules.AdministrativeUnits.Entities;

/// <summary>
/// Hình học ranh giới của một đơn vị hành chính.
/// </summary>
/// <remarks>
/// Tách khỏi <see cref="AdministrativeUnit"/> vì hai lý do: kích thước lớn
/// (một FeatureCollection 34 tỉnh ở độ phân giải đầy đủ có thể tới 20-40MB,
/// kéo nhầm vào endpoint danh sách là lỗi hiệu năng nặng nhất của loại hệ
/// thống này), và vòng đời khác nhau — hình học có thể được thay bằng nguồn
/// có giá trị pháp lý cao hơn mà không đụng tới danh mục.
/// </remarks>
public sealed class AdministrativeBoundary
{
    public Guid UnitId { get; set; }

    /// <summary>Ranh giới ở độ phân giải đầy đủ, EPSG:4326.</summary>
    public required MultiPolygon Boundary { get; set; }

    /// <summary>Bản đơn giản hoá tolerance ~0.01° — dùng khi xem toàn quốc.</summary>
    public MultiPolygon? BoundaryLod1 { get; set; }

    /// <summary>Bản đơn giản hoá tolerance ~0.001° — dùng khi xem cấp tỉnh.</summary>
    public MultiPolygon? BoundaryLod2 { get; set; }

    /// <summary>
    /// Điểm đại diện để đặt nhãn bản đồ. Sinh bằng ST_PointOnSurface chứ
    /// không phải ST_Centroid: với polygon hình chữ C hoặc tỉnh ven biển
    /// nhiều đảo, centroid có thể rơi ra ngoài phần đất của đơn vị.
    /// </summary>
    public required Point Centroid { get; set; }

    /// <summary>Trụ sở Uỷ ban nhân dân, nếu nguồn có cung cấp.</summary>
    public Point? Headquarters { get; set; }

    /// <summary>Hình chữ nhật bao, phục vụ lọc nhanh và canh khung bản đồ.</summary>
    public required Polygon Bbox { get; set; }

    /// <summary>
    /// Diện tích km². Tính bằng ST_Area(boundary::geography)/1e6 — không tính
    /// trực tiếp trên EPSG:4326 vì kết quả sẽ ra độ vuông, vô nghĩa.
    /// </summary>
    public decimal? AreaKm2 { get; set; }

    public required string DataSource { get; set; }

    /// <summary>Ngày nguồn cập nhật hình học này, nếu biết.</summary>
    public DateOnly? SourceUpdated { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public AdministrativeUnit Unit { get; set; } = null!;
}
