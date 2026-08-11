using System.Text.Json;
using CivicFlow.Api.Middleware;
using CivicFlow.Shared.Api;
using CivicFlow.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace CivicFlow.Tests.Api;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task ValidationFailure_Returns400WithPerFieldDetails()
    {
        var failures = new[]
        {
            new ValidationFailure("Email", "Email không hợp lệ."),
            new ValidationFailure("Email", "Email đã tồn tại."),
            new ValidationFailure("FullName", "Họ tên là bắt buộc.")
        };

        var (statusCode, body) = await InvokeAsync(new ValidationException(failures));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);

        var error = Read(body).GetProperty("error");
        Assert.Equal(ErrorCodes.ValidationError, error.GetProperty("code").GetString());

        var details = error.GetProperty("details");
        Assert.Equal(2, details.GetProperty("Email").GetArrayLength());
        Assert.Equal(1, details.GetProperty("FullName").GetArrayLength());
    }

    [Fact]
    public async Task ExpectedDomainError_UsesItsOwnStatusCode()
    {
        var (statusCode, body) = await InvokeAsync(new NotFoundException("Không tìm thấy phản ánh."));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);

        var error = Read(body).GetProperty("error");
        Assert.Equal(ErrorCodes.NotFound, error.GetProperty("code").GetString());
        Assert.Equal("Không tìm thấy phản ánh.", error.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ForbiddenException_Returns403()
    {
        var (statusCode, body) = await InvokeAsync(new ForbiddenException("Ngoài địa bàn được phân công."));

        Assert.Equal(StatusCodes.Status403Forbidden, statusCode);
        Assert.Equal(ErrorCodes.Forbidden, Read(body).GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnexpectedError_HidesInternalDetailsOutsideDevelopment()
    {
        var (statusCode, body) = await InvokeAsync(
            new InvalidOperationException("Chuỗi kết nối chứa mật khẩu bí mật"),
            environmentName: Environments.Production);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);

        var error = Read(body).GetProperty("error");
        Assert.Equal(ErrorCodes.InternalError, error.GetProperty("code").GetString());
        Assert.DoesNotContain("mật khẩu bí mật", error.GetProperty("message").GetString());
    }

    [Fact]
    public async Task UnexpectedError_RevealsDetailsInDevelopment()
    {
        var (_, body) = await InvokeAsync(
            new InvalidOperationException("Chi tiết để gỡ lỗi"),
            environmentName: Environments.Development);

        Assert.Equal(
            "Chi tiết để gỡ lỗi",
            Read(body).GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task ErrorEnvelope_AlwaysCarriesCorrelationId()
    {
        var (_, body) = await InvokeAsync(new NotFoundException("Không có."), correlationId: "trace-xyz");

        var root = Read(body);
        Assert.Equal("trace-xyz", root.GetProperty("error").GetProperty("correlationId").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    private static JsonElement Read(string body) => JsonDocument.Parse(body).RootElement;

    private static async Task<(int StatusCode, string Body)> InvokeAsync(
        Exception exception,
        string environmentName = "Development",
        string correlationId = "test-correlation")
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddOptions().BuildServiceProvider()
        };
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/test";
        context.Items[HttpConstants.CorrelationIdItemKey] = correlationId;
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new FakeHostEnvironment(environmentName));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        return (context.Response.StatusCode, body);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName) => EnvironmentName = environmentName;

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "CivicFlow.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}
