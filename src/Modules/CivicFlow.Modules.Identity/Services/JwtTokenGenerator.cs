using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CivicFlow.Modules.Identity.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CivicFlow.Modules.Identity.Services;

/// <summary>
/// Lớp chịu trách nhiệm khởi tạo JWT Access Token và Refresh Token ngẫu nhiên.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:SecretKey trong ứng dụng.");

        var issuer = _configuration["Jwt:Issuer"] ?? "CivicFlow";
        var audience = _configuration["Jwt:Audience"] ?? "CivicFlowClients";
        var accessTokenMinutes = double.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 60;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(accessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString().ToLowerInvariant()),
            new("administrativeUnitId", user.AdministrativeUnitId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            claims.Add(new(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
