using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Repositories;
using SteamReleaseAnalytics.Services.Services;

using static SteamReleaseAnalytics.Tests.TestData;

namespace SteamReleaseAnalytics.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly IGameRepository _gameRepository = Substitute.For<IGameRepository>();
        private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
        private readonly AnalyticsService _service;

        public AnalyticsServiceTests()
        {
            // По умолчанию любой месяц пустой; конкретные месяцы тесты настраивают сами.
            _gameRepository.GetGamesByMonthAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new List<Game>());
            SetNow(new DateTime(2025, 11, 20, 12, 0, 0, DateTimeKind.Utc));

            _service = new AnalyticsService(_gameRepository, _timeProvider);
        }

        private void SetNow(DateTime utcNow) =>
            _timeProvider.GetUtcNow().Returns(new DateTimeOffset(utcNow));

        private void GivenGames(int year, int month, params Game[] games) =>
            _gameRepository.GetGamesByMonthAsync(year, month).Returns(games.ToList());

        // ---------- GetTopGenresAsync ----------

        [Fact]
        public async Task GetTopGenres_QueriesRepositoryWithYearAndMonthInCorrectOrder()
        {
            GivenGames(2025, 11, Game(1, 100, "Action"));

            var result = await _service.GetTopGenresAsync(month: 11, year: 2025);

            await _gameRepository.Received(1).GetGamesByMonthAsync(2025, 11);
            result.Should().ContainSingle(g => g.Genre == "Action");
        }

        [Fact]
        public async Task GetTopGenres_CountsGamesAndAveragesFollowersPerGenre()
        {
            GivenGames(2025, 11,
                Game(1, 100, "Action", "RPG"),
                Game(2, 300, "Action"),
                Game(3, 50, "Indie"));

            var result = await _service.GetTopGenresAsync(11, 2025);

            result.Should().ContainEquivalentOf(new { Genre = "Action", GameCount = 2, AverageFollowers = 200.0 });
            result.Should().ContainEquivalentOf(new { Genre = "RPG", GameCount = 1, AverageFollowers = 100.0 });
            result.Should().ContainEquivalentOf(new { Genre = "Indie", GameCount = 1, AverageFollowers = 50.0 });
        }

        [Fact]
        public async Task GetTopGenres_OrdersGenresByGameCountDescending()
        {
            GivenGames(2025, 11,
                Game(1, 10, "Indie", "RPG", "Action"),
                Game(2, 10, "RPG", "Action"),
                Game(3, 10, "Action"));

            var result = await _service.GetTopGenresAsync(11, 2025);

            result.Select(g => g.Genre).Should().Equal("Action", "RPG", "Indie");
        }

        [Fact]
        public async Task GetTopGenres_ReturnsOnlyFiveMostPopularGenres()
        {
            // Жанр G{n} встречается в n играх: G7 — самый популярный, G1 — самый редкий.
            var games = Enumerable.Range(1, 7)
                .Select(id => Game(id, 10, Enumerable.Range(id, 8 - id).Select(n => $"G{n}").ToArray()))
                .ToArray();
            GivenGames(2025, 11, games);

            var result = await _service.GetTopGenresAsync(11, 2025);

            result.Select(g => g.Genre).Should().Equal("G7", "G6", "G5", "G4", "G3");
        }

        [Fact]
        public async Task GetTopGenres_GameTaggedTwiceWithSameGenre_IsCountedOnce()
        {
            GivenGames(2025, 11,
                Game(1, 100, "Action", "Action"),
                Game(2, 300, "Action"));

            var result = await _service.GetTopGenresAsync(11, 2025);

            result.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new { Genre = "Action", GameCount = 2, AverageFollowers = 200.0 });
        }

        [Fact]
        public async Task GetTopGenres_NoReleasesInMonth_ReturnsEmptyList()
        {
            var result = await _service.GetTopGenresAsync(11, 2025);

            result.Should().BeEmpty();
        }

        // ---------- GetGenreDynamicsAsync ----------

        [Fact]
        public async Task GetGenreDynamics_QueriesCurrentAndTwoPreviousMonths()
        {
            await _service.GetGenreDynamicsAsync();

            Received.InOrder(() =>
            {
                _gameRepository.GetGamesByMonthAsync(2025, 9);
                _gameRepository.GetGamesByMonthAsync(2025, 10);
                _gameRepository.GetGamesByMonthAsync(2025, 11);
            });
            await _gameRepository.Received(3).GetGamesByMonthAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetGenreDynamics_InJanuary_RollsBackIntoPreviousYear()
        {
            SetNow(new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));
            GivenGames(2025, 11, Game(1, 10, "Action"));
            GivenGames(2025, 12, Game(2, 20, "Action"));
            GivenGames(2026, 1, Game(3, 30, "Action"));

            var result = await _service.GetGenreDynamicsAsync();

            result.Should().ContainSingle()
                .Which.MonthlyStats.Select(m => m.Month).Should().Equal("2025-11", "2025-12", "2026-01");
        }

        [Fact]
        public async Task GetGenreDynamics_BuildsMonthlyStatsOnlyForMonthsWhereGenreAppears()
        {
            GivenGames(2025, 9, Game(1, 100, "Action"));
            GivenGames(2025, 11, Game(2, 100, "Action"), Game(3, 200, "Action", "Puzzle"));

            var result = await _service.GetGenreDynamicsAsync();

            var action = result.Should().ContainSingle(d => d.Genre == "Action").Subject;
            action.MonthlyStats.Should().BeEquivalentTo(new[]
            {
                new { Month = "2025-09", GameCount = 1, AverageFollowers = 100.0 },
                new { Month = "2025-11", GameCount = 2, AverageFollowers = 150.0 }
            }, o => o.WithStrictOrdering());

            var puzzle = result.Should().ContainSingle(d => d.Genre == "Puzzle").Subject;
            puzzle.MonthlyStats.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new { Month = "2025-11", GameCount = 1, AverageFollowers = 200.0 });
        }

        [Fact]
        public async Task GetGenreDynamics_KeepsMostPopularGenreThatAppearsAfterFiveOthers()
        {
            GivenGames(2025, 11,
                Game(1, 10, "A", "B", "C", "D", "E"),
                Game(2, 10, "Big"), Game(3, 10, "Big"), Game(4, 10, "Big"));

            var result = await _service.GetGenreDynamicsAsync();

            result.Should().HaveCount(5);
            result[0].Genre.Should().Be("Big");
            result[0].MonthlyStats.Should().ContainSingle().Which.GameCount.Should().Be(3);
        }

        [Fact]
        public async Task GetGenreDynamics_OrdersByTotalGamesAcrossMonthsAndKeepsTopFive()
        {
            GivenGames(2025, 9,
                Game(1, 10, "A"), Game(2, 10, "B"), Game(3, 10, "C"), Game(4, 10, "D"), Game(5, 10, "E"));
            GivenGames(2025, 10,
                Game(6, 10, "F"), Game(7, 10, "F"), Game(8, 10, "F", "A"));

            var result = await _service.GetGenreDynamicsAsync();

            result.Should().HaveCount(5);
            result[0].Genre.Should().Be("F");
            result[0].MonthlyStats.Sum(m => m.GameCount).Should().Be(3);
            result[1].Genre.Should().Be("A");
            result[1].MonthlyStats.Sum(m => m.GameCount).Should().Be(2);
        }
    }
}
