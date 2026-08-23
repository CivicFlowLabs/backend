using CivicFlow.Modules.Identity.Entities;

namespace CivicFlow.Modules.Identity.DTOs;

/// <summary>
/// Yêu cầu đăng ký tài khoản người dùng mới.
/// </summary>
public record RegisterRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string Password,
    Guid AdministrativeUnitId,
    UserRole Role = UserRole.Citizen);
