using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Tests.Api
{
    /// <summary>
    /// Поведение, которое живёт в конвейере ASP.NET Core, а не в контроллерах.
    /// </summary>
    public class ApiPipelineTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ApiPipelineTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task UnhandledException_ReturnsProblemDetailsWithoutExceptionDetails()
        {
            _factory.Games.GetAllGamesAsync().Returns<List<Core.Models.Game>>(
                _ => throw new InvalidOperationException("connection string leaked here"));

            var response = await _factory.CreateClient().GetAsync("/api/v1/games");

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
            (await response.Content.ReadAsStringAsync()).Should().NotContain("connection string leaked here");
        }

        [Fact]
        public async Task CreateGame_WithOnlyIdAndTitle_IsCreated()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken());

            var response = await client.PostAsJsonAsync("/api/v1/games", new { steamAppId = 7, title = "Minimal" });

            response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateGame_InvalidBody_ReturnsValidationProblem()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken());

            var response = await client.PostAsJsonAsync("/api/v1/games", new { steamAppId = 7, title = "Bad", followers = -1 });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
            (await response.Content.ReadAsStringAsync()).Should().Contain("Followers");
        }

        private string AdminToken()
        {
            using var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateToken("admin", "Admin");
        }
    }
}
