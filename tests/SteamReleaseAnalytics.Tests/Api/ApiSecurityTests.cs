using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Tests.Api
{
    /// <summary>
    /// Аутентификация, роли, Swagger и CORS — через настоящий конвейер API.
    /// </summary>
    public class ApiSecurityTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ApiSecurityTests(ApiFactory factory) => _factory = factory;

        private string Token(string role)
        {
            using var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateToken("someone", role);
        }

        private HttpClient ClientWithToken(string token)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Fact]
        public async Task Login_WithDemoCredentials_ReturnsTokenThatCanCreateGames()
        {
            var login = await _factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login",
                new { username = ApiFactory.DemoUsername, password = ApiFactory.DemoPassword });
            login.StatusCode.Should().Be(HttpStatusCode.OK);
            var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

            var create = await ClientWithToken(token).PostAsJsonAsync("/api/v1/games", new { steamAppId = 11, title = "Game" });

            create.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var login = await _factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login",
                new { username = ApiFactory.DemoUsername, password = "wrong" });

            login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateGame_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _factory.CreateClient().PostAsJsonAsync("/api/v1/games", new { steamAppId = 12, title = "Game" });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateGame_WithNonAdminToken_ReturnsForbidden()
        {
            var response = await ClientWithToken(Token("User")).PostAsJsonAsync("/api/v1/games", new { steamAppId = 13, title = "Game" });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteGame_WithNonAdminToken_ReturnsForbidden()
        {
            var response = await ClientWithToken(Token("User")).DeleteAsync("/api/v1/games/13");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Startup_WithoutJwtSecret_Fails()
        {
            using var factory = _factory.WithWebHostBuilder(b =>
                b.ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string?> { ["JwtSettings:Secret"] = "" })));

            var act = () => factory.CreateClient();

            act.Should().Throw<OptionsValidationException>().Which.OptionsType.Should().Be(typeof(JwtOptions));
        }

        [Fact]
        public async Task Swagger_DeclaresBearerSchemeOnlyOnProtectedOperations()
        {
            var json = await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");

            json.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _).Should().BeTrue();
            var games = json.GetProperty("paths").GetProperty("/api/v1/Games");
            games.GetProperty("post").TryGetProperty("security", out _).Should().BeTrue();
            games.GetProperty("get").TryGetProperty("security", out _).Should().BeFalse();
        }

        [Fact]
        public async Task Swagger_IncludesXmlSummaries()
        {
            var json = await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");

            json.GetProperty("paths").GetProperty("/api/v1/Games").GetProperty("get")
                .GetProperty("summary").GetString().Should().Be("Получить все игры");
        }

        [Theory]
        [InlineData(ApiFactory.AllowedOrigin, true)]
        [InlineData("https://evil.example", false)]
        public async Task Cors_AllowsOnlyConfiguredOrigins(string origin, bool allowed)
        {
            _factory.Games.GetAllGamesAsync().Returns(new List<Core.Models.Game>());
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/games");
            request.Headers.Add("Origin", origin);

            var response = await _factory.CreateClient().SendAsync(request);

            response.Headers.Contains("Access-Control-Allow-Origin").Should().Be(allowed);
        }
    }
}
