using Microsoft.EntityFrameworkCore;

using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Data;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public class GameSnapshotRepository : IGameSnapshotRepository
    {
        private readonly SteamDbContext _context;

        public GameSnapshotRepository(SteamDbContext context)
        {
            _context = context;
        }

        public async Task<List<GameSnapshot>> GetSnapshotsByGameAsync(int steamAppId)
        {
            return await _context.GameSnapshots
                .Where(s => s.GameSteamAppId == steamAppId)
                .OrderBy(s => s.SnapshotDate)
                .ToListAsync();
        }

        public async Task<List<GameSnapshot>> GetSnapshotsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.GameSnapshots
                .Where(s => s.SnapshotDate >= startDate && s.SnapshotDate <= endDate)
                .ToListAsync();
        }

        public async Task AddSnapshotAsync(GameSnapshot snapshot)
        {
            _context.GameSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
        }

        public async Task<List<GameSnapshot>> GetSnapshotsByMonthAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonth = startDate.AddMonths(1);

            return await _context.GameSnapshots
                .Where(s => s.SnapshotDate >= startDate && s.SnapshotDate < nextMonth)
                .ToListAsync();
        }
    }
}