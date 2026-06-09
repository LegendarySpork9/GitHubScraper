// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Models.Related
{
    /// <summary>
    /// Stores information about the workflow run.
    /// </summary>
    public class WorkflowRunModel
    {
        public string? RepositoryName { get; set; }
        public required long Id { get; set; }
        public required int Run_Number { get; set; }
        public required UserModel Actor { get; set; }
        public required string Name { get; set; }
        public required string Display_Title { get; set; }
        public required string Event { get; set; }
        public required string Status { get; set; }
        public required string Conclusion { get; set; }
        public required DateTimeOffset Created_At { get; set; }
        public required DateTimeOffset Updated_At { get; set; }
    }
}
