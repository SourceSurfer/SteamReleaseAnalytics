namespace SteamReleaseAnalytics.Core.Dtos
{
    public class GameDto
    {
        public int SteamAppId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? StoreUrl { get; set; }
        public int Followers { get; set; }
        public string? Platforms { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}