using SteamReleaseAnalytics.Core.Models;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public interface IGameSnapshotRepository
    {
        Task<List<GameSnapshot>> GetSnapshotsByGameAsync(int steamAppId);
        Task<List<GameSnapshot>> GetSnapshotsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task AddSnapshotAsync(GameSnapshot snapshot);
        Task<List<GameSnapshot>> GetSnapshotsByMonthAsync(int year, int month);
    }
}