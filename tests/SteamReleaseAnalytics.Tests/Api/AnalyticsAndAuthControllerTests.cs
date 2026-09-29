using Microsoft.AspNetCore.Mvc;

using SteamReleaseAnalytics.Api.Controllers;
using SteamReleaseAnalytics.Core.Dtos;
using SteamReleaseAnalytics.Services.Security;
using SteamReleaseAnalytics.Services.Services;

namespace SteamReleaseAnalytics.Tests.Api
{
    public class AnalyticsControllerTests
    {
        private readonly IAnalyticsService _analyticsService = Substitute.For<IAnalyticsService>();

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public async Task GetTopGenres_MonthOutOfRange_ReturnsBadRequestWithoutCallingService(int month)
        {
            var response = await new AnalyticsController(_analyticsService).GetTopGenres(month, 2025);

            response.Result.Should().BeOfType<BadRequestObjectResult>();
            await _analyticsService.DidNotReceiveWithAnyArgs().GetTopGenresAsync(default, default);
        }

        [Fact]
        public async Task GetTopGenres_ValidMonth_ReturnsServiceResult()
        {
            var stats = new List<GenreStatsDto> { new() { Genre = "Action", GameCount = 3, AverageFollowers = 120 } };
            _analyticsService.GetTopGenresAsync(12, 2025).Returns(stats);

            var response = await new AnalyticsController(_analyticsService).GetTopGenres(12, 2025);

            response.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(stats);
        }
    }

    public class AuthControllerTests
    {
        private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Login_WithoutUsername_ReturnsBadRequest(string? username)
        {
            var response = new AuthController(_tokenGenerator).Login(new LoginRequest { Username = username! });

            response.Should().BeOfType<BadRequestObjectResult>();
            _tokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default!);
        }

        [Fact]
        public void Login_IssuesAdminTokenForUsername()
        {
            _tokenGenerator.GenerateToken("alice", "Admin").Returns("signed-token");

            var response = new AuthController(_tokenGenerator).Login(new LoginRequest { Username = "alice" });

            response.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { token = "signed-token" });
        }
    }
}
