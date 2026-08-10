using CivicFlow.Api.Filters;
using CivicFlow.Api.Http;
using CivicFlow.Api.Middleware;
using CivicFlow.Modules.Identity;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

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

// --- Tài liệu API -----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.MapControllers();

app.Run();

/// <summary>
/// Lộ lớp Program ra ngoài để WebApplicationFactory trong project test dựng
/// được máy chủ in-memory. Với top-level statements thì lớp này mặc định là
/// internal nên phải khai báo tường minh.
/// </summary>
public partial class Program;
