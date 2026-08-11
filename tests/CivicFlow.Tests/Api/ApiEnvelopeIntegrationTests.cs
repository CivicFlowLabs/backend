using System.Net;
using System.Text.Json;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CivicFlow.Tests.Api;

/// <summary>
/// Kiểm tra ở mức đường ống HTTP thật: những phản hồi phát sinh trước khi
/// request vào tới controller cũng phải theo đúng phong bì chung.
/// Các test này không chạm tới cơ sở dữ liệu.
/// </summary>
public class ApiEnvelopeIntegrationTests : IClassFixture<CivicFlowApiFactory>
{
    private readonly CivicFlowApiFactory _factory;

    public ApiEnvelopeIntegrationTests(CivicFlowApiFactory factory) => _factory = factory;

    [Fact]
    public async Task UnknownPath_ReturnsNotFoundEnvelope()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = (await ReadAsync(response)).GetProperty("error");
        Assert.Equal(ErrorCodes.NotFound, error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task WrongHttpMethod_ReturnsErrorEnvelope()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/health/db", content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.True((await ReadAsync(response)).TryGetProperty("error", out _));
    }

    [Fact]
    public async Task EveryResponse_CarriesCorrelationIdHeader()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");

        Assert.True(response.Headers.TryGetValues(HttpConstants.CorrelationIdHeader, out var values));
        Assert.False(string.IsNullOrWhiteSpace(values!.First()));
    }

    [Fact]
    public async Task ValidClientCorrelationId_IsEchoedBack()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/does-not-exist");
        request.Headers.Add(HttpConstants.CorrelationIdHeader, "user-session-42");

        var response = await client.SendAsync(request);

        Assert.Equal(
            "user-session-42",
            response.Headers.GetValues(HttpConstants.CorrelationIdHeader).Single());
    }

    [Fact]
    public async Task UnsafeCorrelationId_IsReplaced()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/does-not-exist");
        request.Headers.Add(HttpConstants.CorrelationIdHeader, "invalid id <script>");

        var response = await client.SendAsync(request);

        var returned = response.Headers.GetValues(HttpConstants.CorrelationIdHeader).Single();
        Assert.NotEqual("invalid id <script>", returned);
        Assert.Matches("^[A-Za-z0-9]+$", returned);
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
}

/// <summary>
/// Dựng máy chủ in-memory. Chuỗi kết nối chỉ cần tồn tại về mặt cú pháp để
/// module Identity đăng ký được DbContext — các test trong lớp này không mở
/// kết nối thật, nên đây là giá trị giả và không được chứa thông tin đăng
/// nhập của bất kỳ môi trường nào.
/// </summary>
public sealed class CivicFlowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:Default",
            "Host=localhost;Port=5432;Database=civicflow_test;Username=test;Password=test");

        // Ứng dụng cố ý không có khoá JWT mặc định nên sẽ không khởi động
        // nếu thiếu. Đây là khoá dùng một lần cho test, đủ 32 byte theo yêu
        // cầu của HMAC-SHA256, và không được dùng ở bất kỳ môi trường nào.
        builder.UseSetting(
            "Jwt:SecretKey",
            "test-only-signing-key-do-not-use-anywhere-else");
    }
}
