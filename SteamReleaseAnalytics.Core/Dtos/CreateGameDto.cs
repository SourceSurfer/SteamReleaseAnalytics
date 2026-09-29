using System.ComponentModel.DataAnnotations;

namespace SteamReleaseAnalytics.Core.Dtos
{
    public class CreateGameDto : IValidatableObject
    {
        public const int MaxTags = 20;
        public const int MaxTagLength = 100;

        [Range(1, int.MaxValue)]
        public int SteamAppId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [MaxLength(500)]
        [Url]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        [Url]
        public string StoreUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int Followers { get; set; }

        [MaxLength(200)]
        public string Platforms { get; set; }

        public List<string> Tags { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tags.Count > MaxTags)
                yield return new ValidationResult($"Не больше {MaxTags} тегов", new[] { nameof(Tags) });

            if (Tags.Any(string.IsNullOrWhiteSpace))
                yield return new ValidationResult("Тег не может быть пустым", new[] { nameof(Tags) });

            if (Tags.Any(t => t?.Trim().Length > MaxTagLength))
                yield return new ValidationResult($"Тег длиннее {MaxTagLength} символов", new[] { nameof(Tags) });
        }
    }
}
