// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;

namespace GitHubScraper.Implementations
{
    public class FileSystemWrapper : IFileSystem
    {
        /// <summary>
        /// Returns the text in a given file.
        /// </summary>
        public Task<string> ReadAllText(string path) => File.ReadAllTextAsync(path);
    }
}
