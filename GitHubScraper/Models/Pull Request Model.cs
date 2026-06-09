// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Models.Related;

namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores information about the pull request.
    /// </summary>
    public class PullRequestModel
    {
        public string? Repository { get; set; }
        public required long Id { get; set; }
        public required int Number { get; set; }
        public required string Title { get; set; }
        public required UserModel Assignee { get; set; }
        public string? Type { get; set; }
        public required string State { get; set; }
        public required DateTimeOffset Created_At { get; set; }
        public required DateTimeOffset Updated_At { get; set; }
        public DateTimeOffset? Closed_At { get; set; }
        public DateTimeOffset? Merged_At { get; set; }
        public required List<LabelModel> Labels { get; set; } = [];
    }
}
