using Microsoft.AspNetCore.Mvc;

using SteamReleaseAnalytics.Core.Dtos;
using SteamReleaseAnalytics.Services.Services;

namespace SteamReleaseAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Получить топ-5 жанров за месяц с средним количеством фолловеров
        /// </summary>
        [HttpGet("top-genres")]
        public async Task<ActionResult<List<GenreStatsDto>>> GetTopGenres([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12)
                return BadRequest("Месяц должен быть от 1 до 12");

            var stats = await _analyticsService.GetTopGenresAsync(month, year);
            return Ok(stats);
        }

        /// <summary>
        /// Получить динамику изменений топ-5 жанров за последние 3 месяца
        /// </summary>
        [HttpGet("genre-dynamics")]
        public async Task<ActionResult<List<GenreDynamicsDto>>> GetGenreDynamics()
        {
            var dynamics = await _analyticsService.GetGenreDynamicsAsync();
            return Ok(dynamics);
        }
    }
}