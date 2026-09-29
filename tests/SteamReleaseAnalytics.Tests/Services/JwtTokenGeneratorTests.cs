using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Tests.Services
{
    public class JwtTokenGeneratorTests
    {
        private const string Secret = "unit-test-secret-key-that-is-long-enough-for-hmac-sha256";
        private const string Issuer = "SteamReleaseAnalytics";
        private const string Audience = "SteamReleaseAnalyticsUsers";

        // Тот же запасной ключ, что и в Program.cs: сгенерированный токен должен проходить его проверку.
        private const string FallbackSecret = "default-secret-key-change-this-in-production-at-least-32-characters-long!!!";

        private static JwtTokenGenerator CreateGenerator(IDictionary<string, string?> jwtSettings)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(jwtSettings.ToDictionary(kv => $"JwtSettings:{kv.Key}", kv => kv.Value))
                .Build();
            return new JwtTokenGenerator(configuration);
        }

        private static JwtTokenGenerator CreateGenerator(string? expirationMinutes = "30") =>
            CreateGenerator(new Dictionary<string, string?>
            {
                ["Secret"] = Secret,
                ["Issuer"] = Issuer,
                ["Audience"] = Audience,
                ["ExpirationMinutes"] = expirationMinutes
            });

        // Параметры проверки повторяют настройку AddJwtBearer в Program.cs.
        private static ClaimsPrincipal Validate(string token, string secret = Secret) =>
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true
            }, out _);

        [Fact]
        public void GenerateToken_ContainsUsernameAndRoleClaims()
        {
            var token = CreateGenerator().GenerateToken("alice", "Admin");

            var principal = Validate(token);

            principal.FindFirst(ClaimTypes.Name)!.Value.Should().Be("alice");
            principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("alice");
            principal.IsInRole("Admin").Should().BeTrue();
        }

        [Fact]
        public void GenerateToken_WithoutRole_AssignsUserRole()
        {
            var token = CreateGenerator().GenerateToken("bob");

            var principal = Validate(token);

            principal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().Equal("User");
        }

        [Fact]
        public void GenerateToken_UsesIssuerAndAudienceFromConfiguration()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(CreateGenerator().GenerateToken("alice"));

            jwt.Issuer.Should().Be(Issuer);
            jwt.Audiences.Should().Equal(Audience);
        }

        [Fact]
        public void GenerateToken_ExpiresAfterConfiguredMinutes()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(CreateGenerator("15").GenerateToken("alice"));

            jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void GenerateToken_WithoutExpirationSetting_ExpiresAfterSixtyMinutes()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(CreateGenerator(expirationMinutes: null).GenerateToken("alice"));

            jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void GenerateToken_IsRejectedWhenValidatedWithAnotherKey()
        {
            var token = CreateGenerator().GenerateToken("alice");

            var act = () => Validate(token, secret: "some-other-secret-key-that-is-also-long-enough-for-hmac");

            act.Should().Throw<SecurityTokenInvalidSignatureException>();
        }

        [Fact]
        public void GenerateToken_WithoutConfiguredSecret_IsSignedWithFallbackKeyUsedByApi()
        {
            var generator = CreateGenerator(new Dictionary<string, string?>
            {
                ["Issuer"] = Issuer,
                ["Audience"] = Audience
            });

            var principal = Validate(generator.GenerateToken("alice"), secret: FallbackSecret);

            principal.FindFirst(ClaimTypes.Name)!.Value.Should().Be("alice");
        }
    }
}
