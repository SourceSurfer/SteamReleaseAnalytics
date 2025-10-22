using Microsoft.EntityFrameworkCore;

using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Data;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly SteamDbContext _context;

        public GameRepository(SteamDbContext context)
        {
            _context = context;
        }

        public async Task<Game> GetGameByIdAsync(int steamAppId)
        {
            return await _context.Games
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .FirstOrDefaultAsync(g => g.SteamAppId == steamAppId);
        }

        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await _context.Games
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .ToListAsync();
        }

        public async Task<List<Game>> GetGamesByMonthAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.Games
                .Where(g => g.ReleaseDate >= startDate && g.ReleaseDate <= endDate)
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .OrderBy(g => g.ReleaseDate)
                .ToListAsync();
        }

        public async Task<List<Game>> GetGamesByTagAsync(string tagName)
        {
            return await _context.Games
                .Where(g => g.GameTags.Any(gt => gt.Tag.Name == tagName))
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .ToListAsync();
        }

        public async Task AddGameAsync(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGameAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGameAsync(int steamAppId)
        {
            var game = await GetGameByIdAsync(steamAppId);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> GameExistsAsync(int steamAppId)
        {
            return await _context.Games.AnyAsync(g => g.SteamAppId == steamAppId);
        }
    }
}