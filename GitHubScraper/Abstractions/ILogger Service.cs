// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the logger service.
    /// </summary>
    public interface ILoggerService
    {
        void LogMessage(string level, string message);
    }
}
