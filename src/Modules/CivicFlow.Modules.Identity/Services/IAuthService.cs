using CivicFlow.Modules.Identity.DTOs;

namespace CivicFlow.Modules.Identity.Services;

/// <summary>
/// Giao diện định nghĩa các nghiệp vụ xác thực người dùng.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
}
