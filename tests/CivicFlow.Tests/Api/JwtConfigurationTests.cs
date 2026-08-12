using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CivicFlow.Tests.Api;

/// <summary>
/// Bảo vệ một quy tắc an toàn: ứng dụng không được phép khởi động khi thiếu
/// khoá ký JWT hợp lệ. Nếu sau này có ai thêm lại giá trị mặc định trong mã
/// nguồn, các test này sẽ đỏ.
/// </summary>
public class JwtConfigurationTests
{
    [Fact]
    public void MissingSigningKey_PreventsStartup()
    {
        using var factory = new ConfigurableApiFactory(signingKey: null);

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Thiếu cấu hình Jwt:SecretKey", exception.Message);
    }

    [Fact]
    public void SigningKeyShorterThan32Bytes_PreventsStartup()
    {
        using var factory = new ConfigurableApiFactory(signingKey: "qua-ngan");

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("32 byte", exception.Message);
    }

    private sealed class ConfigurableApiFactory : WebApplicationFactory<Program>
    {
        private readonly string? _signingKey;

        public ConfigurableApiFactory(string? signingKey) => _signingKey = signingKey;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Default",
                "Host=localhost;Port=5432;Database=civicflow_test;Username=test;Password=test");

            // Chuỗi rỗng để chắc chắn ghi đè mọi nguồn cấu hình phía dưới,
            // kể cả biến môi trường của máy đang chạy test.
            builder.UseSetting("Jwt:SecretKey", _signingKey ?? string.Empty);
        }
    }
}
