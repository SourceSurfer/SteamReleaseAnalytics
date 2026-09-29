using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SteamReleaseAnalytics.Api.Auth;
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

        [Theory]
        [InlineData(0)]
        [InlineData(1969)]
        [InlineData(2101)]
        public async Task GetTopGenres_YearOutOfRange_ReturnsBadRequestWithoutCallingService(int year)
        {
            var response = await new AnalyticsController(_analyticsService).GetTopGenres(1, year);

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

        private AuthController Controller(string username = "alice", string password = "s3cret") =>
            new(_tokenGenerator, Options.Create(new DemoUserOptions { Username = username, Password = password }));

        [Theory]
        [InlineData(null, "s3cret")]
        [InlineData("", "s3cret")]
        [InlineData("alice", null)]
        [InlineData("alice", "")]
        public void Login_WithoutUsernameOrPassword_ReturnsBadRequest(string? username, string? password)
        {
            var response = Controller().Login(new LoginRequest { Username = username!, Password = password! });

            response.Should().BeOfType<BadRequestObjectResult>();
            _tokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default!);
        }

        [Fact]
        public void Login_WithDemoCredentials_IssuesAdminToken()
        {
            _tokenGenerator.GenerateToken("alice", "Admin").Returns("signed-token");

            var response = Controller().Login(new LoginRequest { Username = "alice", Password = "s3cret" });

            response.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { token = "signed-token" });
        }

        [Theory]
        [InlineData("alice", "wrong")]
        [InlineData("mallory", "s3cret")]
        [InlineData("Alice", "s3cret")]
        public void Login_WithWrongCredentials_ReturnsUnauthorized(string username, string password)
        {
            var response = Controller().Login(new LoginRequest { Username = username, Password = password });

            response.Should().BeOfType<UnauthorizedResult>();
            _tokenGenerator.DidNotReceiveWithAnyArgs().GenerateToken(default!, default!);
        }

        [Fact]
        public void Login_WhenDemoUserIsNotConfigured_RejectsEveryone()
        {
            var response = Controller(username: "", password: "").Login(new LoginRequest { Username = "alice", Password = "s3cret" });

            response.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
