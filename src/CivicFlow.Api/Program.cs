using System.Text;
using CivicFlow.Api.Filters;
using CivicFlow.Api.Http;
using CivicFlow.Api.Middleware;
using CivicFlow.Modules.AdministrativeUnits;
using CivicFlow.Modules.AdministrativeUnits.Persistence;
using CivicFlow.Modules.Identity;
using CivicFlow.Modules.Identity.Persistence;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog (Structured Logging) -------------------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// --- Module nghiệp vụ -------------------------------------------------
// Mỗi module tự đăng ký DbContext và dịch vụ của mình.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddAdministrativeUnitsModule(builder.Configuration);

// Cho phép trả hình học PostGIS ra ngoài dưới dạng GeoJSON chuẩn thay vì
// biểu diễn nội bộ của NetTopologySuite.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()));

// --- MVC --------------------------------------------------------------
// Bộ lọc bọc phong bì được đăng ký toàn cục nên mọi endpoint đều trả về
// cùng một định dạng mà không cần lặp lại ở từng controller.
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseWrappingFilter>())
    .AddApplicationPart(typeof(IdentityModuleExtensions).Assembly)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(entry => entry.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            var response = ApiResponse.Failure(new ApiError
            {
                Code = ErrorCodes.ValidationError,
                Message = "Dữ liệu gửi lên không hợp lệ.",
                Details = details,
                CorrelationId = context.HttpContext.GetCorrelationId()
            });

            return new BadRequestObjectResult(response);
        };
    });

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException(
        "Thiếu cấu hình Jwt:SecretKey. Đặt Jwt__SecretKey trong file .env, "
        + "sinh khoá mới bằng: openssl rand -base64 48");
}


const int MinimumJwtKeyBytes = 32;
if (Encoding.UTF8.GetByteCount(jwtSecretKey) < MinimumJwtKeyBytes)
{
    throw new InvalidOperationException(
        $"Jwt:SecretKey phải dài ít nhất {MinimumJwtKeyBytes} byte để dùng với HMAC-SHA256.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CivicFlow",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CivicFlowClients",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// --- CORS -------------------------------------------------------------
// Đọc origin từ cấu hình để phục vụ ứng dụng di động và ứng dụng máy tính.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(HttpConstants.CorrelationIdHeader));
});

// --- Tài liệu API & Swagger -------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CivicFlow API",
        Version = "v1",
        Description = "Nền tảng dịch vụ hành chính công cấp xã/phường."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer' [khoảng trắng] và nhập JWT token vào ô bên dưới.\r\n\r\nVí dụ: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

var app = builder.Build();

// --- Serilog Request Logging -----------------------------------------
app.UseSerilogRequestLogging();

// --- Tự động áp dụng Migration & Seed Data khi ứng dụng khởi chạy ----
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        using var scope = app.Services.CreateScope();

        // Mỗi module có bộ migration riêng nên phải áp lần lượt từng context.
        var admDbContext = scope.ServiceProvider.GetRequiredService<AdmDbContext>();
        await admDbContext.Database.MigrateAsync();

        var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await identityDbContext.Database.MigrateAsync();

        // Đơn vị hành chính phải có trước, vì tài khoản mẫu tham chiếu tới nó
        // bằng ID logic. Api là nơi duy nhất nhìn thấy cả hai module, nên chỗ
        // ghép hai bên nằm ở đây chứ không nằm trong module nào.
        var defaultUnitId = await AdmDataSeeder.SeedAsync(admDbContext);
        await IdentityDataSeeder.SeedAsync(identityDbContext, defaultUnitId);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Không thể tự động áp dụng Migration hoặc Seed Data khi khởi chạy.");
    }
}

// --- Đường ống xử lý request -----------------------------------------
// Mã tương quan phải nằm ngoài cùng để middleware xử lý lỗi đọc được nó.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    response.ContentType = "application/json";

    await response.WriteAsJsonAsync(
        ApiResponse.Failure(ApiErrorFactory.Create(
            response.StatusCode,
            statusCodeContext.HttpContext.GetCorrelationId())),
        statusCodeContext.HttpContext.RequestAborted);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
