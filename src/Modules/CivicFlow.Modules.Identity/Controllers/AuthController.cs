using CivicFlow.Modules.Identity.DTOs;
using CivicFlow.Modules.Identity.Services;
using CivicFlow.Shared.Api;
using CivicFlow.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.Modules.Identity.Controllers;

/// <summary>
/// API Controller xử lý xác thực: Đăng ký, Đăng nhập và Cấp lại Token.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Đăng ký tài khoản người dùng mới.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Đăng nhập bằng Email hoặc Số điện thoại + Mật khẩu.
    /// Trả về JWT Access Token và Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Cấp lại Access Token mới bằng Refresh Token (Áp dụng Token Rotation).
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request, ipAddress, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Lấy thông tin ngữ cảnh người dùng hiện tại từ Token JWT.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public IActionResult GetMe()
    {
        var info = new
        {
            _currentUser.UserId,
            _currentUser.FullName,
            _currentUser.Email,
            _currentUser.Role,
            _currentUser.AdministrativeUnitId,
            _currentUser.IsAdmin,
            _currentUser.IsLeader,
            _currentUser.IsOfficer,
            _currentUser.IsCitizen
        };

        return Ok(ApiResponse.Success(info));
    }

    /// <summary>
    /// Kiểm tra phân quyền truy cập địa bàn (Territory Access Control).
    /// Cán bộ (Officer) chỉ được truy cập dữ liệu của đúng đơn vị mình.
    /// Ném lỗi HTTP 403 Forbidden nếu Officer cố tình truy cập đơn vị khác.
    /// </summary>
    [HttpGet("territory-check/{administrativeUnitId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public IActionResult TerritoryCheck(Guid administrativeUnitId)
    {
        // Ràng buộc kiểm tra an toàn địa bàn
        _currentUser.EnforceTerritoryAccess(administrativeUnitId);

        var response = new
        {
            Message = "Bạn có quyền truy cập dữ liệu của địa bàn này.",
            TargetAdministrativeUnitId = administrativeUnitId,
            UserRole = _currentUser.Role,
            UserAdministrativeUnitId = _currentUser.AdministrativeUnitId
        };

        return Ok(ApiResponse.Success(response));
    }
}
