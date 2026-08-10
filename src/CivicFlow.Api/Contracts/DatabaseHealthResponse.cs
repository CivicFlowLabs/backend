namespace CivicFlow.Api.Contracts;

/// <summary>
/// Kết quả kiểm tra hạ tầng dữ liệu. Là DTO riêng của tầng API nên có thể
/// đổi mà không ảnh hưởng tới entity bên dưới.
/// </summary>
/// <param name="Database">Trạng thái kết nối cơ sở dữ liệu.</param>
/// <param name="PostGis">Phiên bản PostGIS đang chạy, null nếu chưa bật.</param>
/// <param name="AdministrativeUnits">Số đơn vị hành chính đang có.</param>
public sealed record DatabaseHealthResponse(
    string Database,
    string? PostGis,
    int AdministrativeUnits);
