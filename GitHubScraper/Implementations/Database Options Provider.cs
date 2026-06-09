// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Models;

namespace GitHubScraper.Implementations
{
    public class DatabaseOptionsProvider : IDatabaseOptions
    {
        /// <summary>
        /// Returns the ConnectionString from AppSettings.
        /// </summary>
        public string ConnectionString => AppSettingsModel.ConnectionString;

        /// <summary>
        /// Returns the SQLFiles from AppSettings.
        /// </summary>
        public string SQLFiles => AppSettingsModel.SQLFiles;
    }
}
