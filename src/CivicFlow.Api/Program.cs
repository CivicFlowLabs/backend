using CivicFlow.Modules.Identity;
using CivicFlow.Modules.Identity.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Module nghiệp vụ -------------------------------------------------
// Mỗi module tự đăng ký DbContext và dịch vụ của mình.
builder.Services.AddIdentityModule(builder.Configuration);

// --- CORS -------------------------------------------------------------
// Đọc origin từ cấu hình để phục vụ web và ứng dụng máy tính.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// --- Tài liệu API -----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// --- Endpoint kiểm tra sức khoẻ --------------------------------------
// Dùng để nghiệm thu BE-03: xác nhận kết nối được CSDL và PostGIS đã bật.
app.MapGet("/api/v1/health/db", async (IdentityDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    if (!canConnect)
    {
        return Results.Problem("Không kết nối được cơ sở dữ liệu.");
    }

    var postgisVersion = await db.Database
        .SqlQuery<string>($"SELECT PostGIS_Version() AS \"Value\"")
        .FirstOrDefaultAsync();

    var unitCount = await db.AdministrativeUnits.CountAsync();

    return Results.Ok(new
    {
        database = "ok",
        postgis = postgisVersion,
        administrativeUnits = unitCount
    });
})
.WithName("CheckDatabaseHealth");

app.Run();
