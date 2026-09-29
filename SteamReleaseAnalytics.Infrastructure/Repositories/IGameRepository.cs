using SteamReleaseAnalytics.Core.Models;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public interface IGameRepository
    {
        Task<Game?> GetGameByIdAsync(int steamAppId);
        Task<List<Game>> GetAllGamesAsync();
        Task<List<Game>> GetGamesByMonthAsync(int year, int month);
        Task<List<Game>> GetGamesByTagAsync(string tagName);
        Task AddGameAsync(Game game);
        Task UpdateGameAsync(Game game);
        Task DeleteGameAsync(int steamAppId);
        Task<bool> GameExistsAsync(int steamAppId);
    }
}