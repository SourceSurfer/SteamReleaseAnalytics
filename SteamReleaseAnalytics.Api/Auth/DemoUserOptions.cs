namespace SteamReleaseAnalytics.Api.Auth
{
    /// <summary>
    /// Единственная демо-учётка (секция Auth:DemoUser). Если она не задана, вход отключён.
    /// </summary>
    public class DemoUserOptions
    {
        public const string SectionName = "Auth:DemoUser";

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool IsConfigured => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
    }
}
