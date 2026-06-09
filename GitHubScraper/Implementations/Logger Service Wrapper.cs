// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Services;

namespace GitHubScraper.Implementations
{
    public class LoggerServiceWrapper : ILoggerService
    {
        readonly LoggerService _Logger = new();

        /// <summary>
        /// Logs the given message to the log file.
        /// </summary>
        public void LogMessage(string level, string message)
        {
            _Logger.LogMessage(level, message);
        }
    }
}
