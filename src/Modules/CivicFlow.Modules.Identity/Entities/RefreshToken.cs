using CivicFlow.Shared.Common;

namespace CivicFlow.Modules.Identity.Entities;

/// <summary>Phiên đăng nhập, phục vụ cấp lại access token khi hết hạn.</summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser? User { get; set; }

    public required string Token { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Có giá trị nghĩa là token đã bị thu hồi trước hạn.</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Đủ 45 ký tự để chứa địa chỉ IPv6.</summary>
    public string? CreatedByIp { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
