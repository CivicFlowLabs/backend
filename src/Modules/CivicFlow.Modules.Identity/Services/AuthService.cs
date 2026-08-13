using CivicFlow.Modules.Identity.DTOs;
using CivicFlow.Modules.Identity.Entities;
using CivicFlow.Modules.Identity.Persistence;
using CivicFlow.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CivicFlow.Modules.Identity.Services;

/// <summary>
/// Cài đặt nghiệp vụ đăng ký, đăng nhập và cấp lại token cho người dùng.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IdentityDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;

    public AuthService(
        IdentityDbContext db,
        IJwtTokenGenerator tokenGenerator,
        IConfiguration configuration)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new BadRequestException("Vui lòng cung cấp ít nhất Email hoặc Số điện thoại để đăng ký.");
        }

        // Kiểm tra xem Email đã tồn tại chưa
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _db.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
        {
            throw new ConflictException("Email này đã được sử dụng.");
        }

        // Kiểm tra xem Số điện thoại đã tồn tại chưa
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _db.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken))
        {
            throw new ConflictException("Số điện thoại này đã được sử dụng.");
        }

        if (request.AdministrativeUnitId == Guid.Empty)
        {
            throw new BadRequestException("Mã đơn vị hành chính không được để trống.");
        }

        // Mã hóa mật khẩu bằng BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = passwordHash,
            Role = request.Role,
            AdministrativeUnitId = request.AdministrativeUnitId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync(cancellationToken);

        // Sinh cặp Token đăng nhập ngay sau khi đăng ký
        return await GenerateAuthResponseAsync(newUser, null, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = request.EmailOrPhone.Trim().ToLowerInvariant();

        // Tìm người dùng theo Email hoặc Số điện thoại
        var user = await _db.Users
            .FirstOrDefaultAsync(x =>
                (x.Email != null && x.Email.ToLower() == input) ||
                (x.PhoneNumber != null && x.PhoneNumber == input),
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        // Xác thực mật khẩu BCrypt
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        // Sinh cặp Access Token và Refresh Token mới
        return await GenerateAuthResponseAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenString = request.RefreshToken.Trim();

        // Tìm Refresh Token trong CSDL
        var refreshToken = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == tokenString, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive || refreshToken.User is null || !refreshToken.User.IsActive)
        {
            throw new BadRequestException("Refresh Token không hợp lệ hoặc đã hết hạn.");
        }

        // Thu hồi (Revoke) Refresh Token cũ để áp dụng cơ chế Token Rotation
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        _db.RefreshTokens.Update(refreshToken);

        // Sinh cặp Access Token và Refresh Token mới
        return await GenerateAuthResponseAsync(refreshToken.User, ipAddress, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(AppUser user, string? ipAddress, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = _tokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var refreshTokenDays = double.TryParse(_configuration["Jwt:RefreshTokenDays"], out var days) ? days : 14;

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenDays),
            CreatedByIp = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.AdministrativeUnitId);

        return new AuthResponse(accessToken, refreshTokenString, expiresAt, userDto);
    }
}
