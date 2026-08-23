namespace CivicFlow.Modules.Identity.DTOs;

/// <summary>
/// Yêu cầu đăng nhập tài khoản bằng Email hoặc Số điện thoại.
/// </summary>
public record LoginRequest(
    string EmailOrPhone,
    string Password);
