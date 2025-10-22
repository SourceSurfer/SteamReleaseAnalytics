namespace SteamReleaseAnalytics.Core.Dtos
{
    public class GenreDynamicsDto
    {
        public string Genre { get; set; }
        public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
    }

    public class MonthlyStatsDto
    {
        public string Month { get; set; }
        public int GameCount { get; set; }
        public double AverageFollowers { get; set; }
    }
}