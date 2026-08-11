using System.Text;
using CivicFlow.Api.Filters;
using CivicFlow.Api.Http;
using CivicFlow.Api.Middleware;
using CivicFlow.Modules.Identity;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

// --- MVC --------------------------------------------------------------
// Bộ lọc bọc phong bì được đăng ký toàn cục nên mọi endpoint đều trả về
// cùng một định dạng mà không cần lặp lại ở từng controller.
builder.Services
    .AddControllers(options => options.Filters.Add<ApiResponseWrappingFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        // Mặc định [ApiController] trả về ValidationProblemDetails, khác với
        // phong bì chung — nên thay bằng định dạng của dự án.
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

// --- Xác thực JWT Bearer & Phân quyền -----------------------------------
// Cố ý KHÔNG có giá trị mặc định. Một khoá dự phòng nằm trong mã nguồn nghĩa
// là khi quên cấu hình, ứng dụng vẫn chạy bình thường bằng khoá mà ai cũng
// đọc được trên kho công khai — và ai có khoá thì tự ký được token với bất kỳ
// vai trò nào. Thà chết lúc khởi động còn hơn chạy với xác thực vô hiệu.
// Dùng IsNullOrWhiteSpace chứ không dùng ?? : biến môi trường đặt thành chuỗi
// rỗng vẫn khác null, nên toán tử ?? sẽ để lọt một khoá rỗng.
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException(
        "Thiếu cấu hình Jwt:SecretKey. Đặt Jwt__SecretKey trong file .env, "
        + "sinh khoá mới bằng: openssl rand -base64 48");
}

// HMAC-SHA256 yêu cầu khoá tối thiểu 256 bit. Kiểm tra ngay lúc khởi động
// thay vì để lỗi nổ ra khi cấp token đầu tiên.
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
    // Chỉ nới lỏng khi phát triển cục bộ; ngoài môi trường đó thì bắt buộc HTTPS.
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

// --- Đường ống xử lý request -----------------------------------------
// Mã tương quan phải nằm ngoài cùng để middleware xử lý lỗi đọc được nó.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Các mã lỗi phát sinh trước khi request vào tới MVC — đường dẫn không tồn
// tại, sai phương thức HTTP — mặc định trả về thân rỗng. Bọc lại để client
// luôn nhận đúng một định dạng.
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

/// <summary>
/// Lộ lớp Program ra ngoài để WebApplicationFactory trong project test dựng
/// được máy chủ in-memory. Với top-level statements thì lớp này mặc định là
/// internal nên phải khai báo tường minh.
/// </summary>
public partial class Program;
