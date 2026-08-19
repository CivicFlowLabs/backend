using System.Security.Claims;
using CivicFlow.Shared.Auth;
using Microsoft.AspNetCore.Http;

namespace CivicFlow.Infrastructure.Auth;

/// <summary>
/// Lớp cài đặt ICurrentUser giúp trích xuất thông tin tài khoản đang đăng nhập từ HttpContext Claims.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var claimVal = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claimVal, out var id) ? id : null;
        }
    }

    public string? FullName => User?.FindFirstValue(ClaimTypes.Name);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => User?.FindFirstValue(ClaimTypes.Role)?.ToLowerInvariant();

    public Guid? AdministrativeUnitId
    {
        get
        {
            var claimVal = User?.FindFirstValue(AuthConstants.Claims.AdministrativeUnitId);
            return Guid.TryParse(claimVal, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsAdmin => string.Equals(Role, AuthConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase);

    public bool IsLeader => string.Equals(Role, AuthConstants.Roles.Leader, StringComparison.OrdinalIgnoreCase);

    public bool IsOfficer => string.Equals(Role, AuthConstants.Roles.Officer, StringComparison.OrdinalIgnoreCase);

    public bool IsCitizen => string.Equals(Role, AuthConstants.Roles.Citizen, StringComparison.OrdinalIgnoreCase);

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        return string.Equals(Role, role.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
