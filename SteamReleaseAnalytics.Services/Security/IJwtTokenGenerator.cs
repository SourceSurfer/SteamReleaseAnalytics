namespace SteamReleaseAnalytics.Services.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string username, string role = "User");
    }
}