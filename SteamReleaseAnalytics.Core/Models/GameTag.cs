using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamReleaseAnalytics.Core.Models
{
    public class GameTag
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Game")]
        public int GameSteamAppId { get; set; }

        public Game Game { get; set; } = null!;

        [ForeignKey("Tag")]
        public int TagId { get; set; }

        public Tag Tag { get; set; } = null!;
    }
}