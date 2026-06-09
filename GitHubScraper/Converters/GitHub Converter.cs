// Copyright © - 16/03/2026 - Toby Hunter
namespace GitHubScraper.Converters
{
    public static class GitHubConverter
    {
        /// <summary>
        /// Returns the query parameters for a given endpoint.
        /// </summary>
        public static string GetQuery(string endpoint)
        {
            return endpoint switch
            {
                "/issues" => "?state=all&sort=updated&per_page=100",
                "/branches" => "?per_page=100",
                "/commits" => "?per_page=100",
                "/pulls" => "?state=all&sort=updated&direction=desc&per_page=100",
                "/runs" => "?per_page=100",
                "/releases" => "?per_page=100",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Returns whether the label is an issue type.
        /// </summary>
        public static bool IsType(string label)
        {
            return label switch
            {
                "bug" => true,
                "enhancement" => true,
                "documentation" => true,
                _ => false
            };
        }

        /// <summary>
        /// Returns the correct issue type for the label.
        /// </summary>
        public static string GetType(string label)
        {
            return label switch
            {
                "bug" => "Bug",
                "enhancement" => "New Feature",
                "documentation" => "Documentation",
                _ => label
            };
        }
    }
}
