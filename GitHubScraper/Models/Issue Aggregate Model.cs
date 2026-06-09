// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores the issue counts.
    /// </summary>
    public class IssueAggregateModel
    {
        public DateTime Date { get; set; }
        public int Created { get; set; }
        public int Solved { get; set; }
    }
}
