namespace SteamReleaseAnalytics.Core.Dtos
{
    public class GameCalendarDto
    {
        public string Month { get; set; } = string.Empty;
        public List<DayDto> Days { get; set; } = new();
    }

    public class DayDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}