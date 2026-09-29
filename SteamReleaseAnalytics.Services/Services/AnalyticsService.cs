using SteamReleaseAnalytics.Core.Dtos;
using SteamReleaseAnalytics.Infrastructure.Repositories;

namespace SteamReleaseAnalytics.Services.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IGameRepository _gameRepository;
        private readonly TimeProvider _timeProvider;

        public AnalyticsService(
            IGameRepository gameRepository,
            TimeProvider timeProvider)
        {
            _gameRepository = gameRepository;
            _timeProvider = timeProvider;
        }

        public async Task<List<GenreStatsDto>> GetTopGenresAsync(int month, int year)
        {
            var games = await _gameRepository.GetGamesByMonthAsync(year, month);

            var genreStats = games
                .SelectMany(g => g.GameTags.Select(gt => new { Genre = gt.Tag.Name, Game = g }))
                .GroupBy(g => g.Genre)
                .Select(group => new GenreStatsDto
                {
                    Genre = group.Key,
                    GameCount = group.Select(g => g.Game).Distinct().Count(),
                    AverageFollowers = group.Select(g => g.Game).Distinct().Average(g => g.Followers)
                })
                .OrderByDescending(g => g.GameCount)
                .Take(5)
                .ToList();

            return genreStats;
        }

        public async Task<List<GenreDynamicsDto>> GetGenreDynamicsAsync()
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var months = new[]
            {
                new { Year = now.AddMonths(-2).Year, Month = now.AddMonths(-2).Month },
                new { Year = now.AddMonths(-1).Year, Month = now.AddMonths(-1).Month },
                new { Year = now.Year, Month = now.Month }
            };

            var dynamics = new Dictionary<string, GenreDynamicsDto>();

            foreach (var monthInfo in months)
            {
                var games = await _gameRepository.GetGamesByMonthAsync(monthInfo.Year, monthInfo.Month);
                var monthStr = $"{monthInfo.Year}-{monthInfo.Month:D2}";

                // Все жанры месяца: топ-5 выбирается ниже по сумме за три месяца
                var genreStats = games
                    .SelectMany(g => g.GameTags.Select(gt => new { Genre = gt.Tag.Name, Game = g }))
                    .GroupBy(g => g.Genre);

                foreach (var group in genreStats)
                {
                    if (!dynamics.ContainsKey(group.Key))
                    {
                        dynamics[group.Key] = new GenreDynamicsDto { Genre = group.Key };
                    }

                    dynamics[group.Key].MonthlyStats.Add(new MonthlyStatsDto
                    {
                        Month = monthStr,
                        GameCount = group.Select(g => g.Game).Distinct().Count(),
                        AverageFollowers = group.Select(g => g.Game).Distinct().Average(g => g.Followers)
                    });
                }
            }

            return dynamics.Values.OrderByDescending(d => d.MonthlyStats.Sum(m => m.GameCount)).Take(5).ToList();
        }
    }
}