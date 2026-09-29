using System.Net;

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
    }
}
