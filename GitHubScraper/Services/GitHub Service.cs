// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using System.Globalization;

namespace GitHubScraper.Services
{
    public class GitHubService
    {
        private readonly ILoggerService _Logger;
        private readonly IGitHubClient _GitHubClient;

        // Sets the class's global variables.
        public GitHubService(
            ILoggerService _logger,
            IGitHubClient gitHubClient)
        {
            _Logger = _logger;
            _GitHubClient = gitHubClient;
        }

        /// <summary>
        /// Returns a list of the issues for the repository.
        /// </summary>
        public async Task<List<IssueModel>> GetIssues(string repository, DateTime lastRunDate)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching issues from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");

            List<IssueModel> issues = await _GitHubClient.GetIssues(repository, lastRunDate);
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filtering out pull requests in favour of pull request endpoint data");

            int preFilterIssueCount = issues.Count;

            issues = [.. issues.Where(i => i.Pull_Request == null)];

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filtered out {preFilterIssueCount - issues.Count} pull request(s) in favour of pull request endpoint data");

            foreach (IssueModel issue in issues)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filling blanks for issue {issue.Number}");

                issue.Repository = repository;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repository: {repository}");

                foreach (LabelModel label in issue.Labels)
                {
                    if (GitHubConverter.IsType(label.Name))
                    {
                        issue.Type = GitHubConverter.GetType(label.Name);

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Type: {issue.Type}");

                        break;
                    }
                }

                issue.State = textInfo.ToTitleCase(issue.State);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Status: {issue.State}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filled blanks for issue {issue.Number}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converting date times to UTC for issue {issue.Number}");

                issue.Created_At = issue.Created_At.UtcDateTime;

                if (issue.Closed_At.HasValue)
                {
                    issue.Closed_At = issue.Closed_At.Value.UtcDateTime;
                }

                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converted date times to UTC for issue {issue.Number}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {issues.Count} issue(s) from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            return [.. issues.OrderBy(i => i.Id)];
        }

        /// <summary>
        /// Returns a list of the branches for the repository.
        /// </summary>
        public async Task<List<BranchModel>> GetBranches(string repository)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching branches from GitHub for {repository} repository");

            List<BranchModel> branches = await _GitHubClient.GetBranches(repository);

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {branches.Count} branch(s) from GitHub for {repository} repository");
            return branches;
        }

        /// <summary>
        /// Returns a list of the commits for the repository.
        /// </summary>
        public async Task<List<CommitModel>> GetCommits(string repository, DateTime lastRunDate, string sha)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching commits from GitHub for {repository} repository, {sha} branch from {lastRunDate:dd/MM/yyyy HH:mm:ss}");

            List<CommitModel> commits = await _GitHubClient.GetCommits(repository, lastRunDate, sha);

            foreach (CommitModel commit in commits)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filling blanks for commit {commit.Sha}");

                commit.Repository = repository;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repository: {repository}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filled blanks for commit {commit.Sha}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {commits.Count} commit(s) from GitHub for {repository} repository, {sha} branch from {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            return [.. commits.OrderBy(c => c.Commit.Committer.Date)];
        }

        /// <summary>
        /// Returns a list of the pull requests for the repository.
        /// </summary>
        public async Task<List<PullRequestModel>> GetPullRequests(string repository, DateTime lastRunDate)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching pull requests from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");

            List<PullRequestModel> pullRequests = await _GitHubClient.GetPullRequests(repository, lastRunDate);
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            foreach (PullRequestModel pullRequest in pullRequests)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filling blanks for pull request {pullRequest.Number}");

                pullRequest.Repository = repository;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repository: {repository}");

                foreach (LabelModel label in pullRequest.Labels)
                {
                    if (GitHubConverter.IsType(label.Name))
                    {
                        pullRequest.Type = GitHubConverter.GetType(label.Name);

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Type: {pullRequest.Type}");

                        break;
                    }
                }

                pullRequest.State = textInfo.ToTitleCase(pullRequest.State);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"State: {pullRequest.State}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filled blanks for pull request {pullRequest.Number}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converting date times to UTC for pull request {pullRequest.Number}");

                pullRequest.Created_At = pullRequest.Created_At.UtcDateTime;
                pullRequest.Updated_At = pullRequest.Updated_At.UtcDateTime;

                if (pullRequest.Merged_At.HasValue)
                {
                    pullRequest.Merged_At = pullRequest.Merged_At.Value.UtcDateTime;
                }

                if (pullRequest.Closed_At.HasValue)
                {
                    pullRequest.Closed_At = pullRequest.Closed_At.Value.UtcDateTime;
                }

                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converted date times to UTC for pull request {pullRequest.Number}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {pullRequests.Count} pull request(s) from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            return [.. pullRequests.OrderBy(pr => pr.Id)];
        }

        /// <summary>
        /// Returns a list of the workflow runs for the repository and workflow.
        /// </summary>
        public async Task<List<WorkflowRunModel>> GetWorkflowRuns(string repository, string workflow, DateTime lastRunDate)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching workflow runs from GitHub for {workflow} workflow in {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");

            List<WorkflowRunModel> workflowRuns = await _GitHubClient.GetWorkflowRuns(repository, workflow, lastRunDate);
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            foreach (WorkflowRunModel workflowRun in workflowRuns)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filling blanks for workflow run {workflowRun.Run_Number}");

                workflowRun.RepositoryName = repository;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repository: {repository}");

                workflowRun.Status = textInfo.ToTitleCase(workflowRun.Status);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Status: {workflowRun.Status}");

                workflowRun.Conclusion = textInfo.ToTitleCase(workflowRun.Conclusion);

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Conclusion: {workflowRun.Conclusion}");

                workflowRun.Event = textInfo.ToTitleCase(workflowRun.Event.Replace("_", " "));

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Event: {workflowRun.Event}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filled blanks for workflow run {workflowRun.Run_Number}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converting date times to UTC for workflow run {workflowRun.Run_Number}");

                workflowRun.Created_At = workflowRun.Created_At.UtcDateTime;
                workflowRun.Updated_At = workflowRun.Updated_At.UtcDateTime;

                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converted date times to UTC for workflow run {workflowRun.Run_Number}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {workflowRuns.Count} workflow run(s) from GitHub for {workflow} workflow in {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            return workflowRuns = [.. workflowRuns.OrderBy(wr => wr.Id)];
        }

        /// <summary>
        /// Returns a list of the releases for the repository.
        /// </summary>
        public async Task<List<ReleaseModel>> GetReleases(string repository, DateTime lastRunDate)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetching releases from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");

            List<ReleaseModel> releases = await _GitHubClient.GetReleases(repository, lastRunDate);

            foreach (ReleaseModel release in releases)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filling blanks for release {release.Id}");

                release.Repository = repository;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repository: {repository}");

                release.NumberOfAssets = release.Assets.Count;

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Number Of Assets: {release.NumberOfAssets}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filled blanks for release {release.Id}");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converting date times to UTC for release {release.Id}");

                release.Created_At = release.Created_At.UtcDateTime;
                release.Updated_At = release.Updated_At.UtcDateTime;

                if (release.Published_At.HasValue)
                {
                    release.Published_At = release.Published_At.Value.UtcDateTime;
                }

                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Converted date times to UTC for release {release.Id}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Fetched {releases.Count} release(s) from GitHub for {repository} repository from {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            return [.. releases.OrderBy(r => r.Id)];
        }
    }
}
