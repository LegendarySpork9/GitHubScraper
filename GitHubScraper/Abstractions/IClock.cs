// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the DateTime object.
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime DefaultDate { get; }
    }
}
