// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GitHubScraper.Implementations
{
    public class GitHubClientWrapper : IGitHubClient
    {
        private readonly ILoggerService _Logger;
        private readonly IGitHubOptions _Options;
        private readonly IClock _Clock;
        private readonly IRestClientWrapper _RestClient;

        private readonly string BaseURL = "https://api.github.com";

        // Sets the class's global variables.
        public GitHubClientWrapper(
            ILoggerService _logger,
            IGitHubOptions _options,
            IClock _clock,
            IRestClientWrapper _restClient)
        {
            _Logger = _logger;
            _Options = _options;
            _Clock = _clock;
            _RestClient = _restClient;
        }

        /// <summary>
        /// Returns a list of the issues for the repository.
        /// </summary>
        public async Task<List<IssueModel>> GetIssues(string repository, DateTime lastRunDate)
        {
            List<IssueModel> issues = [];
            int page = 1;

            try
            {
                string url = BuildURL("/issues", repository, lastRunDate);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        List<IssueModel> apiIssues = JsonConvert.DeserializeObject<List<IssueModel>>(response.Content) ?? [];

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Issues Returned: {apiIssues.Count}");

                        if (apiIssues.Count > 0)
                        {
                            issues.AddRange(apiIssues);
                            page++;
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return issues;
        }

        /// <summary>
        /// Returns a list of the branches for the repository.
        /// </summary>
        public async Task<List<BranchModel>> GetBranches(string repository)
        {
            List<BranchModel> branches = [];
            int page = 1;

            try
            {
                string url = BuildURL("/branches", repository, null);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        List<BranchModel> apiBranches = JsonConvert.DeserializeObject<List<BranchModel>>(response.Content) ?? [];

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Branches Returned: {apiBranches.Count}");

                        if (apiBranches.Count > 0)
                        {
                            branches.AddRange(apiBranches);
                            page++;
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return branches;
        }

        /// <summary>
        /// Returns a list of the commits for the repository.
        /// </summary>
        public async Task<List<CommitModel>> GetCommits(string repository, DateTime lastRunDate, string sha)
        {
            List<CommitModel> commits = [];
            int page = 1;

            try
            {
                string url = BuildURL("/commits", repository, lastRunDate);
                url += $"&sha={Uri.EscapeDataString(sha)}";

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        List<CommitModel> apiCommits = JsonConvert.DeserializeObject<List<CommitModel>>(response.Content) ?? [];

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Commits Returned: {apiCommits.Count}");

                        if (apiCommits.Count > 0)
                        {
                            commits.AddRange(apiCommits);
                            page++;
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return commits;
        }

        /// <summary>
        /// Returns a list of the pull requests for the repository.
        /// </summary>
        public async Task<List<PullRequestModel>> GetPullRequests(string repository, DateTime lastRunDate)
        {
            List<PullRequestModel> pullRequests = [];
            int page = 1;

            try
            {
                string url = BuildURL("/pulls", repository, null);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        List<PullRequestModel> apiPullRequests = JsonConvert.DeserializeObject<List<PullRequestModel>>(response.Content) ?? [];

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Pull Requests Returned: {apiPullRequests.Count}");

                        if (apiPullRequests.Count > 0)
                        {
                            int pullRequestsToIgnore = apiPullRequests.Where(apr => apr.Updated_At < lastRunDate).ToList().Count;

                            if (pullRequestsToIgnore > 0)
                            {
                                pullRequests.AddRange(apiPullRequests.Where(apr => apr.Updated_At >= lastRunDate));
                                break;
                            }

                            else
                            {
                                pullRequests.AddRange(apiPullRequests);
                                page++;
                            }
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return pullRequests;
        }

        /// <summary>
        /// Returns a list of the workflow runs for the repository and workflow.
        /// </summary>
        public async Task<List<WorkflowRunModel>> GetWorkflowRuns(string repository, string workflow, DateTime lastRunDate)
        {
            List<WorkflowRunModel> workflowRuns = [];
            int page = 1;

            try
            {
                string url = BuildURL("/actions/workflows", repository, null, workflow);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        JObject responseContent = JObject.Parse(response.Content);
                        int workflowCount = int.Parse(responseContent["total_count"]?.ToString() ?? "0");
                        JToken? workflowRunsToken = responseContent["workflow_runs"];

                        if (workflowCount > 0 && workflowRunsToken != null)
                        {
                            List<WorkflowRunModel> apiWorkflowRuns = JsonConvert.DeserializeObject<List<WorkflowRunModel>>(workflowRunsToken.ToString()) ?? [];

                            _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Workflow Runs Returned: {apiWorkflowRuns.Count}");

                            if (apiWorkflowRuns.Count > 0)
                            {
                                int workflowRunsToIgnore = apiWorkflowRuns.Where(awr => awr.Updated_At < lastRunDate).ToList().Count;

                                if (workflowRunsToIgnore > 0)
                                {
                                    workflowRuns.AddRange(apiWorkflowRuns.Where(awr => awr.Updated_At >= lastRunDate));
                                    break;
                                }

                                else
                                {
                                    workflowRuns.AddRange(apiWorkflowRuns);
                                    page++;
                                }
                            }

                            else
                            {
                                break;
                            }
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return workflowRuns;
        }

        /// <summary>
        /// Returns a list of the releases for the repository.
        /// </summary>
        public async Task<List<ReleaseModel>> GetReleases(string repository, DateTime lastRunDate)
        {
            List<ReleaseModel> releases = [];
            int page = 1;

            try
            {
                string url = BuildURL("/releases", repository, null);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"URL: {url}");

while (true)
                {
                    RestRequest request = new()
                    {
                        Method = Method.Get
                    };
                    request.AddParameter("page", page);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Page: {page}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Configured Rest Request");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Sending Request");

                    request.AddHeader("Authorization", $"Bearer {_Options.BearerToken}");
                    RestResponse response = await _RestClient.ExecuteAsync(url, request);

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Code: {response.StatusCode}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                    {
                        List<ReleaseModel> apiReleases = JsonConvert.DeserializeObject<List<ReleaseModel>>(response.Content) ?? [];

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Releases Returned: {apiReleases.Count}");

                        if (apiReleases.Count > 0)
                        {
                            int releasesToIgnore = apiReleases.Where(ar => ar.Updated_At < lastRunDate).ToList().Count;

                            if (releasesToIgnore > 0)
                            {
                                releases.AddRange(apiReleases.Where(ar => ar.Updated_At >= lastRunDate));
                                break;
                            }

                            else
                            {
                                releases.AddRange(apiReleases);
                                page++;
                            }
                        }

                        else
                        {
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, ex.Message);
                _Logger.LogMessage(StandardValues.LoggerValues.Error, ex.ToString());
            }

            return releases;
        }

        /// <summary>
        /// Returns the API url.
        /// </summary>
        private string BuildURL(string endpoint, string repository, DateTime? lastRunDate, string? workflow = null)
        {
            string url = $"{BaseURL}/repos/{_Options.Owner}/{repository}{endpoint}";
            string query = GitHubConverter.GetQuery(endpoint);

            if (!string.IsNullOrWhiteSpace(workflow))
            {
                url = $"{url}/{workflow}/runs";
                query = GitHubConverter.GetQuery("/runs");

                if (!lastRunDate.HasValue)
                {
                    query += "&created=>2000-01-01T00:00:00Z";
                }

                else
                {
                    query += $"&created=>{_Clock.UtcNow:yyyy-MM-ddT00:00:00Z}";
                }
            }

            if (lastRunDate.HasValue && lastRunDate != _Clock.DefaultDate && string.IsNullOrWhiteSpace(workflow))
            {
                query += $"&since={lastRunDate:yyyy-MM-ddTHH:mm:ssZ}";
            }

            url += query;

            return url;
        }
    }
}
