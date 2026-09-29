using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SteamReleaseAnalytics.Core.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    }
}