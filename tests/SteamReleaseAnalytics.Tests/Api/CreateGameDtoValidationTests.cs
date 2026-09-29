using System.ComponentModel.DataAnnotations;

using SteamReleaseAnalytics.Core.Dtos;

namespace SteamReleaseAnalytics.Tests.Api
{
    /// <summary>
    /// Атрибуты валидации CreateGameDto — те же, что проверяет [ApiController] перед вызовом действия.
    /// </summary>
    public class CreateGameDtoValidationTests
    {
        private static CreateGameDto Valid() => new()
        {
            SteamAppId = 1091500,
            Title = "Cyberpunk 2077",
            Description = "An open-world action RPG",
            ImageUrl = "https://example.com/image.jpg",
            StoreUrl = "https://store.steampowered.com/app/1091500",
            Followers = 150000,
            Platforms = "Windows,Mac",
            Tags = new List<string> { "Action", "RPG" }
        };

        private static IEnumerable<string> InvalidMembers(CreateGameDto dto)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
            return results.SelectMany(r => r.MemberNames);
        }

        [Fact]
        public void ValidDto_HasNoErrors()
        {
            InvalidMembers(Valid()).Should().BeEmpty();
        }

        public static TheoryData<string, Action<CreateGameDto>> InvalidCases => new()
        {
            { nameof(CreateGameDto.SteamAppId), d => d.SteamAppId = 0 },
            { nameof(CreateGameDto.Title), d => d.Title = new string('a', 501) },
            { nameof(CreateGameDto.Description), d => d.Description = new string('a', 2001) },
            { nameof(CreateGameDto.ImageUrl), d => d.ImageUrl = "not a url" },
            { nameof(CreateGameDto.StoreUrl), d => d.StoreUrl = "https://store.example/" + new string('a', 500) },
            { nameof(CreateGameDto.Followers), d => d.Followers = -1 },
            { nameof(CreateGameDto.Platforms), d => d.Platforms = new string('a', 201) },
            { nameof(CreateGameDto.Tags), d => d.Tags = new List<string> { "Action", " " } },
            { nameof(CreateGameDto.Tags), d => d.Tags = new List<string> { new string('a', 101) } },
            { nameof(CreateGameDto.Tags), d => d.Tags = Enumerable.Range(1, 21).Select(i => $"Tag{i}").ToList() }
        };

        [Theory]
        [MemberData(nameof(InvalidCases))]
        public void InvalidField_IsReported(string member, Action<CreateGameDto> spoil)
        {
            var dto = Valid();
            spoil(dto);

            InvalidMembers(dto).Should().Contain(member);
        }
    }
}
