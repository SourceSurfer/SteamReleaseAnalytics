using Microsoft.AspNetCore.Mvc;

using SteamReleaseAnalytics.Api.Controllers;
using SteamReleaseAnalytics.Core.Dtos;
using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Repositories;

using static SteamReleaseAnalytics.Tests.TestData;

namespace SteamReleaseAnalytics.Tests.Api
{
    public class GamesControllerTests
    {
        private readonly IGameRepository _gameRepository = Substitute.For<IGameRepository>();
        private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
        private readonly IGameSnapshotRepository _snapshotRepository = Substitute.For<IGameSnapshotRepository>();
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            _tagRepository.GetOrCreateTagAsync(Arg.Any<string>())
                .Returns(call => new Tag { Name = call.Arg<string>() });

            _controller = new GamesController(_gameRepository, _tagRepository, _snapshotRepository);
        }

        [Fact]
        public async Task GetGameCalendar_GroupsReleasesByDayInDateOrder()
        {
            _gameRepository.GetGamesByMonthAsync(2025, 11).Returns(new List<Game>
            {
                Game(1, 10, new DateTime(2025, 11, 20, 18, 0, 0, DateTimeKind.Utc)),
                Game(2, 10, new DateTime(2025, 11, 5, 9, 0, 0, DateTimeKind.Utc)),
                Game(3, 10, new DateTime(2025, 11, 5, 21, 30, 0, DateTimeKind.Utc))
            });

            var response = await _controller.GetGameCalendar("2025-11");

            var calendar = response.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<GameCalendarDto>().Subject;
            calendar.Month.Should().Be("2025-11");
            calendar.Days.Should().BeEquivalentTo(new[]
            {
                new { Date = "2025-11-05", Count = 2 },
                new { Date = "2025-11-20", Count = 1 }
            }, o => o.WithStrictOrdering());
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("2025-13")]
        public async Task GetGameCalendar_InvalidMonth_ReturnsBadRequestWithoutQuery(string month)
        {
            var response = await _controller.GetGameCalendar(month);

            response.Result.Should().BeOfType<BadRequestObjectResult>();
            await _gameRepository.DidNotReceiveWithAnyArgs().GetGamesByMonthAsync(default, default);
        }

        [Fact]
        public async Task GetGameById_UnknownId_ReturnsNotFound()
        {
            _gameRepository.GetGameByIdAsync(42).Returns((Game)null!);

            var response = await _controller.GetGameById(42);

            response.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetGameById_MapsEntityToDtoWithTagNames()
        {
            var release = new DateTime(2025, 11, 5, 0, 0, 0, DateTimeKind.Utc);
            _gameRepository.GetGameByIdAsync(42).Returns(Game(42, 1500, release, "Action", "RPG"));

            var response = await _controller.GetGameById(42);

            var dto = response.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<GameDto>().Subject;
            dto.Should().BeEquivalentTo(new
            {
                SteamAppId = 42,
                Title = "Game 42",
                Followers = 1500,
                ReleaseDate = (DateTime?)release,
                Platforms = "Windows",
                Tags = new[] { "Action", "RPG" }
            });
        }

        [Fact]
        public async Task CreateGame_ExistingSteamAppId_ReturnsBadRequestAndDoesNotSave()
        {
            _gameRepository.GameExistsAsync(42).Returns(true);

            var response = await _controller.CreateGame(new CreateGameDto { SteamAppId = 42, Title = "Duplicate" });

            response.Result.Should().BeOfType<BadRequestObjectResult>();
            await _gameRepository.DidNotReceiveWithAnyArgs().AddGameAsync(default!);
        }

        [Fact]
        public async Task CreateGame_SavesGameWithTagsAndReturnsCreatedDto()
        {
            var dto = new CreateGameDto
            {
                SteamAppId = 42,
                Title = "New Game",
                Followers = 700,
                Platforms = "Windows,Linux",
                Tags = new List<string> { "Action", "Indie" }
            };

            var response = await _controller.CreateGame(dto);

            await _tagRepository.Received(1).GetOrCreateTagAsync("Action");
            await _tagRepository.Received(1).GetOrCreateTagAsync("Indie");
            await _gameRepository.Received(1).AddGameAsync(Arg.Is<Game>(g =>
                g.SteamAppId == 42 &&
                g.Title == "New Game" &&
                g.Followers == 700 &&
                g.GameTags.Select(gt => gt.Tag.Name).SequenceEqual(new[] { "Action", "Indie" })));

            var created = response.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(GamesController.GetGameById));
            created.RouteValues!["id"].Should().Be(42);
            created.Value.Should().BeOfType<GameDto>()
                .Which.Tags.Should().Equal("Action", "Indie");
        }

        [Fact]
        public async Task DeleteGame_UnknownId_ReturnsNotFoundAndDeletesNothing()
        {
            _gameRepository.GameExistsAsync(42).Returns(false);

            var response = await _controller.DeleteGame(42);

            response.Should().BeOfType<NotFoundResult>();
            await _gameRepository.DidNotReceiveWithAnyArgs().DeleteGameAsync(default);
        }

        [Fact]
        public async Task DeleteGame_ExistingId_DeletesAndReturnsNoContent()
        {
            _gameRepository.GameExistsAsync(42).Returns(true);

            var response = await _controller.DeleteGame(42);

            response.Should().BeOfType<NoContentResult>();
            await _gameRepository.Received(1).DeleteGameAsync(42);
        }
    }
}
