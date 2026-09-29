using SteamReleaseAnalytics.Core.Models;

namespace SteamReleaseAnalytics.Tests
{
    /// <summary>
    /// Построители тестовых сущностей: игра с тегами собирается одной строкой.
    /// </summary>
    internal static class TestData
    {
        public static Game Game(int id, int followers, DateTime? releaseDate, params string[] tags)
        {
            var game = new Game
            {
                SteamAppId = id,
                Title = $"Game {id}",
                Description = $"Description {id}",
                ReleaseDate = releaseDate,
                ImageUrl = $"https://img.example/{id}.jpg",
                StoreUrl = $"https://store.example/app/{id}",
                Followers = followers,
                Platforms = "Windows"
            };

            foreach (var tag in tags)
            {
                game.GameTags.Add(new GameTag { Game = game, Tag = new Tag { Name = tag } });
            }

            return game;
        }

        public static Game Game(int id, int followers, params string[] tags) =>
            Game(id, followers, new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc), tags);
    }
}
