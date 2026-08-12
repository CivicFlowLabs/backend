using CivicFlow.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.Identity.Persistence;

/// <summary>
/// Khởi tạo dữ liệu mẫu cho module Identity khi cơ sở dữ liệu chưa có dữ liệu.
/// </summary>
public static class IdentityDataSeeder
{
    /// <summary>
    /// Nạp tài khoản mẫu nếu bảng người dùng đang trống.
    /// </summary>
    /// <param name="context">Context của module Identity.</param>
    /// <param name="administrativeUnitId">
    /// Khoá chính của đơn vị hành chính gán cho các tài khoản mẫu. Truyền từ
    /// ngoài vào chứ không tự tra cứu: đơn vị hành chính do module khác sở
    /// hữu, mà module không được phụ thuộc lẫn nhau. Bên gọi — tầng Api — là
    /// nơi duy nhất nhìn thấy cả hai module nên nó chịu trách nhiệm ghép.
    /// </param>
    /// <param name="ct">Thẻ huỷ tác vụ.</param>
    public static async Task SeedAsync(
        IdentityDbContext context,
        Guid administrativeUnitId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (await context.Users.AnyAsync(ct))
        {
            return;
        }

        var adminUser = new AppUser
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            FullName = "Quản trị viên Hệ thống",
            Email = "admin@civicflow.vn",
            PhoneNumber = "0900000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            Role = UserRole.Admin,
            AdministrativeUnitId = administrativeUnitId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var officerUser = new AppUser
        {
            Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
            FullName = "Cán bộ Phường Bến Nghé",
            Email = "canbo.bennghe@civicflow.vn",
            PhoneNumber = "0900000002",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Officer@123456"),
            Role = UserRole.Officer,
            AdministrativeUnitId = administrativeUnitId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var citizenUser = new AppUser
        {
            Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
            FullName = "Nguyễn Văn Dân",
            Email = "citizen@civicflow.vn",
            PhoneNumber = "0900000003",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Citizen@123456"),
            Role = UserRole.Citizen,
            AdministrativeUnitId = administrativeUnitId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.AddRange(adminUser, officerUser, citizenUser);
        await context.SaveChangesAsync(ct);
    }
}
