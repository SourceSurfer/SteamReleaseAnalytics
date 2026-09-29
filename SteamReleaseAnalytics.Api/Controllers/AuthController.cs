using Microsoft.AspNetCore.Mvc;

using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthController(IJwtTokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }

        /// <summary>
        /// Получить JWT токен для тестирования
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Username))
                return BadRequest("Username is required");

            var token = _tokenGenerator.GenerateToken(request.Username, "Admin");
            return Ok(new { token });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
    }
}