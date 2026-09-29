using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SteamReleaseAnalytics.Core.Models
{
    public class Game
    {
        [Key]
        public int SteamAppId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? StoreUrl { get; set; }

        public int Followers { get; set; }

        public string? Platforms { get; set; } // "Windows,Mac,Linux"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();

        public ICollection<GameSnapshot> Snapshots { get; set; } = new List<GameSnapshot>();
    }
}