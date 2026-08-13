// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Functions;
using GitHubScraper.Implementations;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using System.Configuration;

namespace GitHubScraper.Services
{
    public class ApplicationService
    {
        private readonly ILoggerService _Logger;

        // Sets the class's global variables.
        public ApplicationService(
            ILoggerService _logger)
        {
            _Logger = _logger;
        }

        /// <summary>
        /// Checks the application settings are present.
        /// </summary>
        public bool Setup()
        {
            bool configured = true;

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Owner"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid owner not found. Please provide one in the app settings with the tag \"Owner\"");

                configured = false;
            }

            else
            {
                AppSettingsModel.Owner = ConfigurationManager.AppSettings["Owner"];

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Owner: {AppSettingsModel.Owner}");
            }

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Repositories"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid repositories not found. Please provide one in the app settings with the tag \"Repositories\"");

                configured = false;
            }

            else
            {
                AppSettingsModel.Repositories = ConfigurationManager.AppSettings["Repositories"].Split(',');

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Repositories: {AppSettingsModel.Repositories.Length}");
            }

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Workflows"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid workflows not found. Please provide them in the app settings with the tag \"Workflows\" if required");
            }

            else
            {
                AppSettingsModel.Workflows = ConfigurationManager.AppSettings["Workflows"].Split(',');

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Workflows: {AppSettingsModel.Workflows.Length}");
            }

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["BearerToken"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid authentication token not found. Please provide one in the app settings with the tag \"BearerToken\"");

                configured = false;
            }

            else
            {
                AppSettingsModel.BearerToken = ConfigurationManager.AppSettings["BearerToken"];

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Bearer Token: {AppSettingsModel.BearerToken}");
            }

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SQLConnectionString"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid connection string not found. Please provide one in the app settings with the tag \"SQLConnectionString\"");

                configured = false;
            }

            else
            {
                AppSettingsModel.ConnectionString = ConfigurationManager.AppSettings["SQLConnectionString"];

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Connection String: {AppSettingsModel.ConnectionString}");
            }

            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SQLFiles"]))
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, "Valid sql files not found. Please provide one in the app settings with the tag \"SQLFiles\"");

                configured = false;
            }

            else
            {
                AppSettingsModel.SQLFiles = ConfigurationManager.AppSettings["SQLFiles"];

                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"SQL Files: {AppSettingsModel.SQLFiles}");
            }

            return configured;
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        public async Task Run()
        {
            IClock _clock = new SystemClockProvider();
            IDatabaseOptions _options = new DatabaseOptionsProvider();
            DatabaseService _databaseService = new(_Logger, _clock, new FileSystemWrapper(), _options, new DatabaseWrapper(_options, _Logger));
            GitHubService _gitHubService = new(_Logger, new GitHubClientWrapper(_Logger, new GitHubOptionsProvider(), _clock));
            DatabaseFunction _databaseFunction = new(_Logger, _clock);

            foreach (string repository in AppSettingsModel.Repositories)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Running Scraper for {repository}");

                DateTime lastRunDate = await _databaseService.GetLastRunDate(repository);
                List<IssueModel>? existingIssues = await _databaseService.GetIssues(repository);

                if (existingIssues == null)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        $"Skipping {repository} due to failed existing issues lookup");

                    continue;
                }

                List<IssueModel> issues = await _gitHubService.GetIssues(repository, lastRunDate);
                List<BranchModel> branches = await _gitHubService.GetBranches(repository);
                List<CommitModel> commits = [];

                foreach (BranchModel branch in branches)
                {
                    List<CommitModel> branchCommits = await _gitHubService.GetCommits(repository, lastRunDate, branch.Name);

                    if (branchCommits.Count > 0)
                    {
                        commits.AddRange(branchCommits);
                    }
                }

                commits = [.. commits.DistinctBy(c => c.Sha).OrderBy(c => c.Commit.Committer.Date)];
                List<PullRequestModel> pullRequests = await _gitHubService.GetPullRequests(repository, lastRunDate);
                List<WorkflowModel> workflows = [];

                int totalWorkflowRuns = 0;

                if (!string.IsNullOrWhiteSpace(string.Join(',', AppSettingsModel.Workflows)))
                {
                    foreach (string workflow in AppSettingsModel.Workflows)
                    {
                        List<WorkflowRunModel> workflowRuns = await _gitHubService.GetWorkflowRuns(repository, workflow, lastRunDate);

                        if (workflowRuns.Count > 0)
                        {
                            workflows.Add(new()
                            {
                                Name = workflow,
                                WorkflowRuns = workflowRuns
                            });

                            totalWorkflowRuns += workflowRuns.Count;
                        }
                    }
                }

                List<ReleaseModel> releases = await _gitHubService.GetReleases(repository, lastRunDate);

                await _databaseService.OutputIssues(repository, issues);
                await _databaseService.OutputCommits(repository, commits);
                await _databaseService.OutputPullRequests(repository, pullRequests);

                foreach (WorkflowModel workflow in workflows)
                {
                    await _databaseService.OutputWorkflowRuns(repository, workflow);
                }

                await _databaseService.OutputReleases(repository, releases);

                List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates(repository, _databaseFunction.FilterIssues(repository, issues, existingIssues), existingIssues);

                await _databaseService.LogIssueAggregates(repository, issueAggregates);
                await _databaseService.LogRun(repository, issues.Count, commits.Count, pullRequests.Count, totalWorkflowRuns, releases.Count);

                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Ran Scraper for {repository}");
            }
        }
    }
}
