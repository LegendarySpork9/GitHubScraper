// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Models;

namespace GitHubScraper.Implementations
{
    public class GitHubOptionsProvider : IGitHubOptions
    {
        /// <summary>
        /// Returns the Owner from AppSettings.
        /// </summary>
        public string Owner => AppSettingsModel.Owner;

        /// <summary>
        /// Returns the Bearer Token from AppSettings.
        /// </summary>
        public string BearerToken => AppSettingsModel.BearerToken;
    }
}
