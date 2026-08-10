namespace CivicFlow.Shared.Api;

/// <summary>
/// Tiền tố đường dẫn dùng chung cho mọi controller. Gom về một chỗ để khi
/// lên phiên bản mới chỉ phải sửa duy nhất tại đây.
/// </summary>
public static class ApiRoutes
{
    public const string Version = "v1";

    /// <summary>Gốc của API, ví dụ "api/v1".</summary>
    public const string Base = "api/" + Version;

    /// <summary>
    /// Dùng trực tiếp trong thuộc tính Route của controller:
    /// <c>[Route(ApiRoutes.Controller)]</c> sẽ thành "api/v1/reports"
    /// với ReportsController.
    /// </summary>
    public const string Controller = Base + "/[controller]";
}
