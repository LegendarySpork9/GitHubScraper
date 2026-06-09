// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the file system operations.
    /// </summary>
    public interface IFileSystem
    {
        Task<string> ReadAllText(string path);
    }
}
