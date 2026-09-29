using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SteamReleaseAnalytics.Infrastructure.Repositories;

namespace SteamReleaseAnalytics.Tests.Api
{
    /// <summary>
    /// Поднимает API целиком (конвейер, фильтры, авторизация, Swagger) в среде Production,
    /// но с подменёнными репозиториями — PostgreSQL не нужен.
    /// </summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string DemoUsername = "demo";
        public const string DemoPassword = "demo-password";
        public const string AllowedOrigin = "https://frontend.example";

        public IGameRepository Games { get; } = Substitute.For<IGameRepository>();
        public ITagRepository Tags { get; } = Substitute.For<ITagRepository>();
        public IGameSnapshotRepository Snapshots { get; } = Substitute.For<IGameSnapshotRepository>();

        public static Dictionary<string, string?> Settings() => new()
        {
            ["JwtSettings:Secret"] = "integration-test-secret-key-long-enough-for-hmac-sha256",
            ["Auth:DemoUser:Username"] = DemoUsername,
            ["Auth:DemoUser:Password"] = DemoPassword,
            ["Cors:AllowedOrigins:0"] = AllowedOrigin
        };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(Settings()));
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => Games);
                services.AddScoped(_ => Tags);
                services.AddScoped(_ => Snapshots);
            });
        }
    }
}
