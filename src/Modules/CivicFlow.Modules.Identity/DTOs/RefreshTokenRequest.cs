namespace CivicFlow.Modules.Identity.DTOs;

/// <summary>
/// Yêu cầu xin cấp lại Access Token bằng Refresh Token.
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken);
