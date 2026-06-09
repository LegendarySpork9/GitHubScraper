// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Models.Related;

namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores information about the commit.
    /// </summary>
    public class CommitModel
    {
        public string? Repository { get; set; }
        public string? Sha { get; set; }
        public required RelatedCommitModel Commit { get; set; }
    }
}
