// Copyright © - 16/03/2026 - Toby Hunter
using RestSharp;

namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the REST client operations.
    /// </summary>
    public interface IRestClientWrapper
    {
        Task<RestResponse> ExecuteAsync(string url, RestRequest request);
    }
}
