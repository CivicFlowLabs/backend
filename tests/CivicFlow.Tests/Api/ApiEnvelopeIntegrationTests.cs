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
    public async Task DuongDanKhongTonTai_TraVePhongBiLoi404()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/khong-ton-tai");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = (await ReadAsync(response)).GetProperty("error");
        Assert.Equal(ErrorCodes.NotFound, error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SaiPhuongThucHttp_TraVePhongBiLoi405()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/health/db", content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.True((await ReadAsync(response)).TryGetProperty("error", out _));
    }

    [Fact]
    public async Task MoiPhanHoi_DeuCoHeaderMaTuongQuan()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/khong-ton-tai");

        Assert.True(response.Headers.TryGetValues(HttpConstants.CorrelationIdHeader, out var values));
        Assert.False(string.IsNullOrWhiteSpace(values!.First()));
    }

    [Fact]
    public async Task ClientGuiMaTuongQuanHopLe_MayChuNhanLai()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/khong-ton-tai");
        request.Headers.Add(HttpConstants.CorrelationIdHeader, "phien-nguoi-dung-42");

        var response = await client.SendAsync(request);

        Assert.Equal(
            "phien-nguoi-dung-42",
            response.Headers.GetValues(HttpConstants.CorrelationIdHeader).Single());
    }

    [Fact]
    public async Task MaTuongQuanChuaKyTuLa_BiThayBangMaMoi()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/khong-ton-tai");
        request.Headers.Add(HttpConstants.CorrelationIdHeader, "khong hop le <script>");

        var response = await client.SendAsync(request);

        var returned = response.Headers.GetValues(HttpConstants.CorrelationIdHeader).Single();
        Assert.NotEqual("khong hop le <script>", returned);
        Assert.Matches("^[A-Za-z0-9]+$", returned);
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
}

/// <summary>
/// Dựng máy chủ in-memory. Chuỗi kết nối chỉ cần tồn tại để module Identity
/// đăng ký được DbContext — các test trong lớp này không mở kết nối thật.
/// </summary>
public sealed class CivicFlowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("Environment", "Testing");
        builder.UseSetting(
            "ConnectionStrings:Default",
            "Host=localhost;Port=5432;Database=civicflow_test;Username=civicflow;Password=civicflow");
    }
}
