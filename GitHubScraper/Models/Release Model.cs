// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Models.Related;

namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores information about the release.
    /// </summary>
    public class ReleaseModel
    {
        public string? Repository { get; set; }
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required UserModel Author { get; set; }
        public required string Body { get; set; }
        public int NumberOfAssets { get; set; } = 0;
        public required bool Draft { get; set; }
        public required DateTimeOffset Created_At { get; set; }
        public required DateTimeOffset Updated_At { get; set; }
        public DateTimeOffset? Published_At { get; set; }
        public required List<AssetModel> Assets { get; set; } = [];
    }
}
