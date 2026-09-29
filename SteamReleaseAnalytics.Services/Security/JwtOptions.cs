using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace SteamReleaseAnalytics.Services.Security
{
    /// <summary>
    /// Настройки выдачи и проверки JWT (секция JwtSettings). Проверяются при старте приложения.
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// Ключ подписи HMAC-SHA256: не короче 32 символов (256 бит).
        /// </summary>
        [Required]
        [MinLength(32)]
        public string Secret { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 24 * 60)]
        public int ExpirationMinutes { get; set; } = 60;

        public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(Secret));
    }
}
