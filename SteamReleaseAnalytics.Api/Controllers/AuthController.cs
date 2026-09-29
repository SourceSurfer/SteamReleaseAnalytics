using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SteamReleaseAnalytics.Api.Auth;
using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly DemoUserOptions _demoUser;

        public AuthController(IJwtTokenGenerator tokenGenerator, IOptions<DemoUserOptions> demoUser)
        {
            _tokenGenerator = tokenGenerator;
            _demoUser = demoUser.Value;
        }

        /// <summary>
        /// Получить JWT токен демо-пользователя (роль Admin)
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password are required");

            if (!_demoUser.IsConfigured
                || !FixedTimeEquals(request.Username, _demoUser.Username)
                || !FixedTimeEquals(request.Password, _demoUser.Password))
                return Unauthorized();

            var token = _tokenGenerator.GenerateToken(request.Username, Roles.Admin);
            return Ok(new { token });
        }

        // Сравнение за постоянное время: хэши одинаковой длины, время не зависит от совпавшего префикса
        private static bool FixedTimeEquals(string actual, string expected) =>
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
