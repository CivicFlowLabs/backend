using CivicFlow.Modules.Identity.Entities;

namespace CivicFlow.Modules.Identity.Services;

/// <summary>
/// Giao diện tạo JWT Access Token mã hóa cho người dùng.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
}
