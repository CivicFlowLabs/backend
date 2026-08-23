namespace CivicFlow.Modules.Identity.DTOs;

/// <summary>
/// Phản hồi trả về sau khi đăng ký, đăng nhập hoặc refresh token thành công.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserDto User);
