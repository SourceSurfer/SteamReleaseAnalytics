using SteamReleaseAnalytics.Core.Dtos;

namespace SteamReleaseAnalytics.Services.Services
{
    public interface IAnalyticsService
    {
        Task<List<GenreStatsDto>> GetTopGenresAsync(int month, int year);
        Task<List<GenreDynamicsDto>> GetGenreDynamicsAsync();
    }
}