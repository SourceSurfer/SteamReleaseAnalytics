using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamReleaseAnalytics.Core.Models
{
    public class GameSnapshot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Game")]
        public int GameSteamAppId { get; set; }

        public Game Game { get; set; }

        public int FollowersCount { get; set; }

        public DateTime SnapshotDate { get; set; }
    }
}