using CivicFlow.Shared.Common;

namespace CivicFlow.Modules.Identity.Entities;

/// <summary>
/// Người dùng hệ thống. Đặt tên AppUser thay vì User vì "user" là từ khoá
/// dành riêng của PostgreSQL.
/// </summary>
public class AppUser : BaseEntity
{
    public required string FullName { get; set; }

    /// <summary>Cán bộ và lãnh đạo thường đăng nhập bằng email.</summary>
    public string? Email { get; set; }

    /// <summary>Người dân thường đăng nhập bằng số điện thoại.</summary>
    public string? PhoneNumber { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    /// <summary>Địa bàn của người dùng — cơ sở để lọc dữ liệu ở tầng truy vấn.</summary>
    public Guid AdministrativeUnitId { get; set; }

    public AdministrativeUnit? AdministrativeUnit { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
