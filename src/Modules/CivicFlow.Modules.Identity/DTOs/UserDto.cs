using CivicFlow.Modules.Identity.Entities;

namespace CivicFlow.Modules.Identity.DTOs;

/// <summary>
/// DTO chứa thông tin rút gọn của người dùng trả về cho client.
/// </summary>
public record UserDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    UserRole Role,
    Guid AdministrativeUnitId);
