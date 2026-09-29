using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using SteamReleaseAnalytics.Services.Security;

namespace SteamReleaseAnalytics.Tests.Services
{
    public class JwtTokenGeneratorTests
    {
        private const string Secret = "unit-test-secret-key-that-is-long-enough-for-hmac-sha256";
        private const string Issuer = "SteamReleaseAnalytics";
        private const string Audience = "SteamReleaseAnalyticsUsers";

        private static JwtOptions Options(int expirationMinutes = 30) => new()
        {
            Secret = Secret,
            Issuer = Issuer,
            Audience = Audience,
            ExpirationMinutes = expirationMinutes
        };

        private static JwtTokenGenerator CreateGenerator(int expirationMinutes = 30) =>
            new(Microsoft.Extensions.Options.Options.Create(Options(expirationMinutes)));

        // Ключ строится тем же JwtOptions.CreateSigningKey, что и в настройке JwtBearer в Program.cs.
        private static ClaimsPrincipal Validate(string token, string secret = Secret) =>
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new JwtOptions { Secret = secret }.CreateSigningKey(),
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
        public void GenerateToken_UsesIssuerAndAudienceFromOptions()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(CreateGenerator().GenerateToken("alice"));

            jwt.Issuer.Should().Be(Issuer);
            jwt.Audiences.Should().Equal(Audience);
        }

        [Fact]
        public void GenerateToken_ExpiresAfterConfiguredMinutes()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(CreateGenerator(15).GenerateToken("alice"));

            jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void JwtOptions_DefaultExpiration_IsSixtyMinutes()
        {
            new JwtOptions().ExpirationMinutes.Should().Be(60);
        }

        [Fact]
        public void GenerateToken_IsRejectedWhenValidatedWithAnotherKey()
        {
            var token = CreateGenerator().GenerateToken("alice");

            var act = () => Validate(token, secret: "some-other-secret-key-that-is-also-long-enough-for-hmac");

            act.Should().Throw<SecurityTokenInvalidSignatureException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("too-short-for-hmac-sha256")]
        public void JwtOptions_WithMissingOrShortSecret_IsInvalid(string secret)
        {
            var options = Options();
            options.Secret = secret;

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true)
                .Should().BeFalse();
            results.SelectMany(r => r.MemberNames).Should().Contain(nameof(JwtOptions.Secret));
        }

        [Fact]
        public void JwtOptions_Complete_IsValid()
        {
            var options = Options();

            Validator.TryValidateObject(options, new ValidationContext(options), new List<ValidationResult>(), validateAllProperties: true)
                .Should().BeTrue();
        }
    }
}
