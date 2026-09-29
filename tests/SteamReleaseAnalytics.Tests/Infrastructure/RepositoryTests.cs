using Microsoft.EntityFrameworkCore;

using SteamReleaseAnalytics.Infrastructure.Data;
using SteamReleaseAnalytics.Infrastructure.Repositories;

using static SteamReleaseAnalytics.Tests.TestData;

namespace SteamReleaseAnalytics.Tests.Infrastructure
{
    /// <summary>
    /// Репозитории поверх EF Core InMemory: отдельная база на каждый тест, без PostgreSQL.
    /// Запись и чтение идут через разные экземпляры контекста, чтобы не проверять трекер вместо запроса.
    /// </summary>
    public class RepositoryTests
    {
        private readonly DbContextOptions<SteamDbContext> _options = new DbContextOptionsBuilder<SteamDbContext>()
            .UseInMemoryDatabase($"steam-tests-{Guid.NewGuid()}")
            .Options;

        private SteamDbContext NewContext() => new(_options);

        private static DateTime Utc(int year, int month, int day, int hour = 0) =>
            new(year, month, day, hour, 0, 0, DateTimeKind.Utc);

        private async Task SeedAsync(params Core.Models.Game[] games)
        {
            await using var context = NewContext();
            context.Games.AddRange(games);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetGamesByMonth_ReturnsOnlyReleasesOfThatMonthOrderedByDate()
        {
            await SeedAsync(
                Game(1, 10, Utc(2025, 11, 15)),
                Game(2, 10, Utc(2025, 10, 31, 23)),
                Game(3, 10, Utc(2025, 11, 1)),
                Game(4, 10, Utc(2025, 12, 1)),
                Game(5, 10, Utc(2025, 11, 30)),
                Game(6, 10, releaseDate: null));

            await using var context = NewContext();
            var games = await new GameRepository(context).GetGamesByMonthAsync(2025, 11);

            games.Select(g => g.SteamAppId).Should().Equal(3, 1, 5);
        }

        [Fact]
        public async Task GetGamesByMonth_IncludesReleasesLaterOnTheLastDayOfMonth()
        {
            await SeedAsync(
                Game(1, 10, Utc(2025, 11, 30, 15)),
                Game(2, 10, Utc(2025, 12, 1)));

            await using var context = NewContext();
            var games = await new GameRepository(context).GetGamesByMonthAsync(2025, 11);

            games.Select(g => g.SteamAppId).Should().Equal(1);
        }

        [Fact]
        public async Task GetSnapshotsByMonth_IncludesSnapshotsLaterOnTheLastDayOfMonth()
        {
            await SeedAsync(Game(1, 10));
            await using (var seed = NewContext())
            {
                seed.GameSnapshots.AddRange(
                    new Core.Models.GameSnapshot { GameSteamAppId = 1, FollowersCount = 5, SnapshotDate = Utc(2025, 11, 30, 15) },
                    new Core.Models.GameSnapshot { GameSteamAppId = 1, FollowersCount = 6, SnapshotDate = Utc(2025, 12, 1) });
                await seed.SaveChangesAsync();
            }

            await using var context = NewContext();
            var snapshots = await new GameSnapshotRepository(context).GetSnapshotsByMonthAsync(2025, 11);

            snapshots.Select(s => s.FollowersCount).Should().Equal(5);
        }

        [Fact]
        public async Task GetGamesByMonth_LoadsTagsOfReturnedGames()
        {
            await SeedAsync(Game(1, 10, Utc(2025, 11, 15), "Action", "RPG"));

            await using var context = NewContext();
            var games = await new GameRepository(context).GetGamesByMonthAsync(2025, 11);

            games.Should().ContainSingle()
                .Which.GameTags.Select(gt => gt.Tag.Name).Should().BeEquivalentTo("Action", "RPG");
        }

        [Fact]
        public async Task GetGamesByTag_ReturnsOnlyGamesWithThatTag()
        {
            await SeedAsync(
                Game(1, 10, "Action", "RPG"),
                Game(2, 10, "Puzzle"),
                Game(3, 10, "RPG"));

            await using var context = NewContext();
            var games = await new GameRepository(context).GetGamesByTagAsync("RPG");

            games.Select(g => g.SteamAppId).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task DeleteGame_RemovesGameAndIgnoresUnknownId()
        {
            await SeedAsync(Game(1, 10, "Action"), Game(2, 10));

            await using (var context = NewContext())
            {
                var repository = new GameRepository(context);
                await repository.DeleteGameAsync(1);
                await repository.DeleteGameAsync(999);
            }

            await using var check = NewContext();
            var repositoryAfter = new GameRepository(check);
            (await repositoryAfter.GameExistsAsync(1)).Should().BeFalse();
            (await repositoryAfter.GameExistsAsync(2)).Should().BeTrue();
        }

        [Fact]
        public async Task GetOrCreateTag_CreatesTagOnceAndReusesItAfterwards()
        {
            int firstId;
            await using (var context = NewContext())
            {
                var created = await new TagRepository(context).GetOrCreateTagAsync("Roguelike");
                await context.SaveChangesAsync();
                firstId = created.Id;
            }

            await using var second = NewContext();
            var again = await new TagRepository(second).GetOrCreateTagAsync("Roguelike");

            again.Id.Should().Be(firstId);
            (await second.Tags.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task GetOrCreateTag_DoesNotSaveNewTagOnItsOwn()
        {
            await using (var context = NewContext())
            {
                await new TagRepository(context).GetOrCreateTagAsync("Roguelike");
            }

            await using var check = NewContext();
            (await check.Tags.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetOrCreateTag_SameNewNameTwiceBeforeSave_ReturnsSameTag()
        {
            await using var context = NewContext();
            var repository = new TagRepository(context);

            var first = await repository.GetOrCreateTagAsync("Roguelike");
            var second = await repository.GetOrCreateTagAsync("Roguelike");

            second.Should().BeSameAs(first);
        }

        [Fact]
        public async Task AddGame_SavesNewTagsTogetherWithGame()
        {
            await using (var context = NewContext())
            {
                var tag = await new TagRepository(context).GetOrCreateTagAsync("Roguelike");
                var game = Game(1, 10);
                game.GameTags.Add(new Core.Models.GameTag { Game = game, Tag = tag });
                await new GameRepository(context).AddGameAsync(game);
            }

            await using var check = NewContext();
            var saved = await new GameRepository(check).GetGameByIdAsync(1);
            saved!.GameTags.Select(gt => gt.Tag.Name).Should().Equal("Roguelike");
            (await check.Tags.CountAsync()).Should().Be(1);
        }
    }
}
