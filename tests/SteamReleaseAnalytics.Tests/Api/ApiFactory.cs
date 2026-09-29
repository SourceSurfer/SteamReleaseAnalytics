using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
        public IGameRepository Games { get; } = Substitute.For<IGameRepository>();
        public ITagRepository Tags { get; } = Substitute.For<ITagRepository>();
        public IGameSnapshotRepository Snapshots { get; } = Substitute.For<IGameSnapshotRepository>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => Games);
                services.AddScoped(_ => Tags);
                services.AddScoped(_ => Snapshots);
            });
        }
    }
}
