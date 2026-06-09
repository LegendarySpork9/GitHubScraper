// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the GitHub Options.
    /// </summary>
    public interface IGitHubOptions
    {
        string Owner { get; }
        string BearerToken { get; }
    }
}
