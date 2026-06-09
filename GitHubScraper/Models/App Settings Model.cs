// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores the settings used by the application.
    /// </summary>
    public static class AppSettingsModel
    {
        public static string? Owner { get; set; }
        public static string[]? Repositories { get; set; }
        public static string[]? Workflows { get; set; }
        public static string? BearerToken { get; set; }
        public static string? ConnectionString { get; set; }
        public static string? SQLFiles { get; set; }
    }
}
