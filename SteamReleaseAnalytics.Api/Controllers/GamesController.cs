using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SteamReleaseAnalytics.Api.Auth;
using SteamReleaseAnalytics.Api.Validation;
using SteamReleaseAnalytics.Core.Dtos;
using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Repositories;

namespace SteamReleaseAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepository _gameRepository;
        private readonly ITagRepository _tagRepository;

        public GamesController(
            IGameRepository gameRepository,
            ITagRepository tagRepository)
        {
            _gameRepository = gameRepository;
            _tagRepository = tagRepository;
        }

        /// <summary>
        /// Получить все игры
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GameDto>>> GetAllGames()
        {
            var games = await _gameRepository.GetAllGamesAsync();
            var dtos = games.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Получить игру по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGameById(int id)
        {
            var game = await _gameRepository.GetGameByIdAsync(id);
            if (game == null)
                return NotFound();

            return Ok(MapToDto(game));
        }

        /// <summary>
        /// Получить игры по месяцу (формат: 2025-11)
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult<GameCalendarDto>> GetGameCalendar([FromQuery] string month)
        {
            if (!ReleasePeriod.TryParseMonth(month, out var date))
                return BadRequest($"Неверный месяц. Используйте формат YYYY-MM, {ReleasePeriod.YearRangeMessage.ToLowerInvariant()}");

            var games = await _gameRepository.GetGamesByMonthAsync(date.Year, date.Month);

            var calendarDto = new GameCalendarDto
            {
                Month = month,
                Days = games
                    .GroupBy(g => g.ReleaseDate?.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DayDto
                    {
                        Date = g.Key?.ToString("yyyy-MM-dd") ?? "",
                        Count = g.Count()
                    })
                    .ToList()
            };

            return Ok(calendarDto);
        }

        /// <summary>
        /// Получить игры по тегу
        /// </summary>
        [HttpGet("by-tag/{tagName}")]
        public async Task<ActionResult<List<GameDto>>> GetGamesByTag(string tagName)
        {
            var games = await _gameRepository.GetGamesByTagAsync(tagName);
            var dtos = games.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Создать новую игру
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameDto createDto)
        {
            if (await _gameRepository.GameExistsAsync(createDto.SteamAppId))
                return BadRequest("Игра с таким ID уже существует");

            var game = new Game
            {
                SteamAppId = createDto.SteamAppId,
                Title = createDto.Title,
                Description = createDto.Description,
                ReleaseDate = ToUtc(createDto.ReleaseDate),
                ImageUrl = createDto.ImageUrl,
                StoreUrl = createDto.StoreUrl,
                Followers = createDto.Followers,
                Platforms = createDto.Platforms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Новые теги сохраняются вместе с игрой одним SaveChanges
            foreach (var tagName in createDto.Tags.Select(t => t.Trim()).Distinct())
            {
                var tag = await _tagRepository.GetOrCreateTagAsync(tagName);
                game.GameTags.Add(new GameTag { Tag = tag });
            }

            await _gameRepository.AddGameAsync(game);

            return CreatedAtAction(nameof(GetGameById), new { id = game.SteamAppId }, MapToDto(game));
        }

        /// <summary>
        /// Удалить игру
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteGame(int id)
        {
            if (!await _gameRepository.GameExistsAsync(id))
                return NotFound();

            await _gameRepository.DeleteGameAsync(id);
            return NoContent();
        }

        // PostgreSQL хранит timestamptz только в UTC; дата без часового пояса считается UTC
        private static DateTime? ToUtc(DateTime? value) => value switch
        {
            null => null,
            { Kind: DateTimeKind.Local } local => local.ToUniversalTime(),
            { } other => DateTime.SpecifyKind(other, DateTimeKind.Utc)
        };

        private GameDto MapToDto(Game game)
        {
            return new GameDto
            {
                SteamAppId = game.SteamAppId,
                Title = game.Title,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                ImageUrl = game.ImageUrl,
                StoreUrl = game.StoreUrl,
                Followers = game.Followers,
                Platforms = game.Platforms,
                Tags = game.GameTags.Select(gt => gt.Tag.Name).ToList()
            };
        }
    }
}