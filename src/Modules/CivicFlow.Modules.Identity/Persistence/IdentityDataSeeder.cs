using CivicFlow.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Modules.Identity.Persistence;

/// <summary>
/// Khởi tạo dữ liệu mẫu cho module Identity khi cơ sở dữ liệu chưa có dữ liệu.
/// </summary>
public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IdentityDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. Seed Đơn vị hành chính nếu bảng đang trống
        if (!await context.AdministrativeUnits.AnyAsync())
        {
            var adminUnitBenNghe = new AdministrativeUnit
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "76001",
                Name = "Phường Bến Nghé",
                Type = AdministrativeUnitType.Ward,
                ProvinceCode = "79",
                ProvinceName = "Thành phố Hồ Chí Minh",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var adminUnitBenThanh = new AdministrativeUnit
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "76002",
                Name = "Phường Bến Thành",
                Type = AdministrativeUnitType.Ward,
                ProvinceCode = "79",
                ProvinceName = "Thành phố Hồ Chí Minh",
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.AdministrativeUnits.AddRange(adminUnitBenNghe, adminUnitBenThanh);
            await context.SaveChangesAsync();
        }

        // 2. Seed Người dùng mẫu nếu bảng đang trống
        if (!await context.Users.AnyAsync())
        {
            var defaultUnit = await context.AdministrativeUnits.FirstAsync();

            var adminUser = new AppUser
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                FullName = "Quản trị viên Hệ thống",
                Email = "admin@civicflow.vn",
                PhoneNumber = "0900000001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                Role = UserRole.Admin,
                AdministrativeUnitId = defaultUnit.Id,
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
                AdministrativeUnitId = defaultUnit.Id,
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
                AdministrativeUnitId = defaultUnit.Id,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Users.AddRange(adminUser, officerUser, citizenUser);
            await context.SaveChangesAsync();
        }
    }
}
