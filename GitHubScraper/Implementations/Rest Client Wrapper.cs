// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using RestSharp;

namespace GitHubScraper.Implementations
{
    public class RestClientWrapper : IRestClientWrapper
    {
        /// <summary>
        /// Executes the given request against the given URL.
        /// </summary>
        public async Task<RestResponse> ExecuteAsync(string url, RestRequest request)
        {
            RestClient client = new(url);
            return await client.ExecuteAsync(request);
        }
    }
}
