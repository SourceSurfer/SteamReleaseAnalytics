namespace SteamReleaseAnalytics.Core.Dtos
{
    public class GenreStatsDto
    {
        public string Genre { get; set; } = string.Empty;
        public int GameCount { get; set; }
        public double AverageFollowers { get; set; }
    }
}