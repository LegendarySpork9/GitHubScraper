// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Models.Related;

namespace GitHubScraper.Models
{
    /// <summary>
    /// Stores information about the workflow.
    /// </summary>
    public class WorkflowModel
    {
        public required string Name { get; set; }
        public required List<WorkflowRunModel> WorkflowRuns { get; set; } = [];
    }
}
