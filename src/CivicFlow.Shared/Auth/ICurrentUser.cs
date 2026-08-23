using CivicFlow.Shared.Exceptions;

namespace CivicFlow.Shared.Auth;

/// <summary>
/// Giao diện đại diện cho người dùng đang đăng nhập trong ngữ cảnh request hiện tại.
/// Cung cấp thông tin tài khoản, vai trò và cơ chế kiểm tra phân quyền địa bàn.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? FullName { get; }
    string? Email { get; }
    string? Role { get; }
    Guid? AdministrativeUnitId { get; }

    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsLeader { get; }
    bool IsOfficer { get; }
    bool IsCitizen { get; }

    bool IsInRole(string role);

    /// <summary>
    /// Kiểm tra phân quyền truy cập địa bàn (Territory Access Control).
    /// Nếu người dùng là Cán bộ (Officer) mà cố tình truy cập dữ liệu của đơn vị khác,
    /// phương thức sẽ ném ForbiddenException (trả về HTTP 403 Forbidden).
    /// Admin và Leader có quyền truy cập toàn diện.
    /// </summary>
    /// <param name="targetAdministrativeUnitId">Mã đơn vị hành chính cần kiểm tra.</param>
    void EnforceTerritoryAccess(Guid targetAdministrativeUnitId)
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện thao tác này.");
        }

        // Admin và Leader có quyền truy cập rộng
        if (IsAdmin || IsLeader)
        {
            return;
        }

        // Officer chỉ được phép truy cập đúng đơn vị hành chính của mình
        if (IsOfficer)
        {
            if (!AdministrativeUnitId.HasValue || AdministrativeUnitId.Value != targetAdministrativeUnitId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập hoặc thao tác trên dữ liệu của địa bàn khác.");
            }
        }
    }
}
