// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using Microsoft.Data.SqlClient;

namespace GitHubScraper.Services
{
    public class DatabaseService
    {
        private readonly ILoggerService _Logger;
        private readonly IClock _Clock;
        private readonly IFileSystem _FileSystem;
        private readonly IDatabaseOptions _Options;
        private readonly IDatabase _Database;

        // Sets the class's global variables.
        public DatabaseService(
            ILoggerService _logger,
            IClock _clock,
            IFileSystem _fileSystem,
            IDatabaseOptions _options,
            IDatabase _database)
        {
            _Logger = _logger;
            _Clock = _clock;
            _FileSystem = _fileSystem;
            _Options = _options;
            _Database = _database;
        }

        /// <summary>
        /// Gets the last time the application was run for the given repository.
        /// </summary>
        public async Task<DateTime> GetLastRunDate(string repository)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Obtaining the last run date for repository {repository}");

            DateTime lastRunDate = _Clock.DefaultDate;

            try
            {
                string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\GetLastRunDate.sql");
                SqlParameter[] parameters =
                [
                    new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = repository }
                ];

                (DateTime result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                {
                    return DateTime.SpecifyKind(dataReader.GetDateTime(0), DateTimeKind.Utc);
                }, parameters).Result;

                if (ex != null)
                {
                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to obtain the last run date for repository {repository}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }

                if (result != default)
                {
                    lastRunDate = result;
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to obtain the last run date for repository {repository}. Error Message: {ex.Message}");
                _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"Last Run Date: {lastRunDate:dd/MM/yyyy HH:mm:ss}");
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Obtained the last run date for repository {repository}");
            return lastRunDate;
        }

        /// <summary>
        ///  Gets the existing issues for the given repository.
        /// </summary>
        public async Task<List<IssueModel>> GetIssues(string repository)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Obtaining the existing issues for repository {repository}");

            List<IssueModel> existingIssues = [];

            try
            {
                string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\GetIssues.sql");
                SqlParameter[] parameters =
                [
                    new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = repository }
                ];

                (existingIssues, Exception? ex) = _Database.Query(sql, dataReader =>
                {
                    DateTime createdDate = DateTime.SpecifyKind(dataReader.GetDateTime(1), DateTimeKind.Utc);
                    DateTime updatedDate = DateTime.SpecifyKind(dataReader.GetDateTime(2), DateTimeKind.Utc);

                    return new IssueModel
                    {
                        Id = dataReader.GetInt64(0),
                        Number = 0,
                        Title = "UnLoaded",
                        State = "UnLoaded",
                        Created_At = createdDate,
                        Closed_At = updatedDate,
                        Labels = []
                    };
                }, parameters).Result;

                if (ex != null)
                {
                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to obtain the existing issues for repository {repository}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to obtain the existing issues for repository {repository}. Error Message: {ex.Message}");
                _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{existingIssues.Count} existsing issue(s)");
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Obtained the existing issues for repository {repository}");
            return existingIssues;
        }

        /// <summary>
        /// Outputs the issues to the database.
        /// </summary>
        public async Task OutputIssues(string repository, List<IssueModel> issues)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting {issues.Count} issue(s) for repository {repository}");

            List<IssueModel> successful = [];
            List<IssueModel> errored = [];

            foreach (IssueModel issue in issues)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting issue {issue.Number}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\OutputIssue.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = issue.Repository },
                        new SqlParameter("@issueId", System.Data.SqlDbType.BigInt) { Value = issue.Id },
                        new SqlParameter("@number", System.Data.SqlDbType.Int) { Value = issue.Number },
                        new SqlParameter("@title", System.Data.SqlDbType.VarChar) { Value = issue.Title },
                        new SqlParameter("@assignee", System.Data.SqlDbType.VarChar) { Value = issue.Assignee?.Login ?? "Unassigned" },
                        new SqlParameter("@type", System.Data.SqlDbType.VarChar) { Value = issue.Type ?? "Undefined" },
                        new SqlParameter("@status", System.Data.SqlDbType.VarChar) { Value = issue.State },
                        new SqlParameter("@dateCreated", System.Data.SqlDbType.DateTime) { Value = issue.Created_At.UtcDateTime },
                        new SqlParameter("@dateSolved", System.Data.SqlDbType.DateTime) { Value = issue.Closed_At?.UtcDateTime ?? _Clock.DefaultDate }
                    ];

                    (int result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                    {
                        return dataReader.GetInt32(0);
                    }, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(issue);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output issue {issue.Number}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 0)
                    {
                        successful.Add(issue);

                        _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted issue {issue.Number}");
                    }
                }

                catch (Exception ex)
                {
                    errored.Add(issue);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output issue {issue.Number}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (issues.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / issues.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / issues.Count) * 100}%) output errored");
            }
            
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted {issues.Count} issue(s) for repository {repository}");
        }

        /// <summary>
        /// Outputs the commits to the database.
        /// </summary>
        public async Task OutputCommits(string repository, List<CommitModel> commits)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting {commits.Count} commit(s) for repository {repository}");

            List<CommitModel> successful = [];
            List<CommitModel> errored = [];

            foreach (CommitModel commit in commits)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting commit {commit.Sha}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\OutputCommit.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = commit.Repository },
                        new SqlParameter("@author", System.Data.SqlDbType.VarChar) { Value = commit.Commit.Author.Name },
                        new SqlParameter("@committer", System.Data.SqlDbType.VarChar) { Value = commit.Commit.Committer.Name },
                        new SqlParameter("@sha", System.Data.SqlDbType.VarChar) { Value = commit.Sha },
                        new SqlParameter("@message", System.Data.SqlDbType.VarChar) { Value = commit.Commit.Message }
                    ];

                    (int result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                    {
                        return dataReader.GetInt32(0);
                    }, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(commit);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output commit {commit.Sha}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 0)
                    {
                        successful.Add(commit);

                        _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted commit {commit.Sha}");
                    }
                }

                catch (Exception ex)
                {
                    errored.Add(commit);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output commit {commit.Sha}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (commits.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / commits.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / commits.Count) * 100}%) output errored");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted {commits.Count} commit(s) for repository {repository}");
        }

        /// <summary>
        /// Outputs the pull requests to the database.
        /// </summary>
        public async Task OutputPullRequests(string repository, List<PullRequestModel> pullRequests)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting {pullRequests.Count} pull request(s) for repository {repository}");

            List<PullRequestModel> successful = [];
            List<PullRequestModel> errored = [];

            foreach (PullRequestModel pullRequest in pullRequests)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting pull request {pullRequest.Number}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\OutputPullRequest.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = pullRequest.Repository },
                        new SqlParameter("@pullRequestId", System.Data.SqlDbType.BigInt) { Value = pullRequest.Id },
                        new SqlParameter("@number", System.Data.SqlDbType.Int) { Value = pullRequest.Number },
                        new SqlParameter("@title", System.Data.SqlDbType.VarChar) { Value = pullRequest.Title },
                        new SqlParameter("@assignee", System.Data.SqlDbType.VarChar) { Value = pullRequest.Assignee?.Login ?? "Unassigned" },
                        new SqlParameter("@type", System.Data.SqlDbType.VarChar) { Value = pullRequest.Type ?? "Undefined" },
                        new SqlParameter("@status", System.Data.SqlDbType.VarChar) { Value = pullRequest.State },
                        new SqlParameter("@dateCreated", System.Data.SqlDbType.DateTime) { Value = pullRequest.Created_At.UtcDateTime },
                        new SqlParameter("@dateSolved", System.Data.SqlDbType.DateTime) { Value = pullRequest.Closed_At?.UtcDateTime ?? _Clock.DefaultDate },
                        new SqlParameter("@dateMerged", System.Data.SqlDbType.DateTime) { Value = pullRequest.Merged_At?.UtcDateTime ?? _Clock.DefaultDate }
                    ];

                    (int result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                    {
                        return dataReader.GetInt32(0);
                    }, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(pullRequest);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output pull request {pullRequest.Number}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 0)
                    {
                        successful.Add(pullRequest);
                    }

                    _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted pull request {pullRequest.Number}");
                }

                catch (Exception ex)
                {
                    errored.Add(pullRequest);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output pull request {pullRequest.Number}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (pullRequests.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / pullRequests.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / pullRequests.Count) * 100}%) output errored");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted {pullRequests.Count} pull request(s) for repository {repository}");
        }

        /// <summary>
        /// Outputs the workflow runs to the database.
        /// </summary>
        public async Task OutputWorkflowRuns(string repository, WorkflowModel workflow)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting {workflow.WorkflowRuns.Count} workflow run(s) for {workflow.Name} workflow in repository {repository}");

            List<WorkflowRunModel> successful = [];
            List<WorkflowRunModel> errored = [];

            foreach (WorkflowRunModel workflowRun in workflow.WorkflowRuns)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting workflow run {workflowRun.Run_Number}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\OutputWorkflowRun.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = workflowRun.RepositoryName },
                        new SqlParameter("@workflow", System.Data.SqlDbType.VarChar) { Value = workflowRun.Name },
                        new SqlParameter("@workflowRunId", System.Data.SqlDbType.BigInt) { Value = workflowRun.Id },
                        new SqlParameter("@runNumber", System.Data.SqlDbType.Int) { Value = workflowRun.Run_Number },
                        new SqlParameter("@actor", System.Data.SqlDbType.VarChar) { Value = workflowRun.Actor.Login },
                        new SqlParameter("@displayTitle", System.Data.SqlDbType.VarChar) { Value = workflowRun.Display_Title },
                        new SqlParameter("@event", System.Data.SqlDbType.VarChar) { Value = workflowRun.Event },
                        new SqlParameter("@status", System.Data.SqlDbType.VarChar) { Value = workflowRun.Status },
                        new SqlParameter("@conclusion", System.Data.SqlDbType.VarChar) { Value = workflowRun.Conclusion },
                        new SqlParameter("@dateCreated", System.Data.SqlDbType.DateTime) { Value = workflowRun.Created_At.UtcDateTime },
                    ];

                    (int result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                    {
                        return dataReader.GetInt32(0);
                    }, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(workflowRun);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output workflow run {workflowRun.Run_Number}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 0)
                    {
                        successful.Add(workflowRun);

                        _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted workflow run {workflowRun.Run_Number}");
                    }
                }

                catch (Exception ex)
                {
                    errored.Add(workflowRun);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output workflow run {workflowRun.Run_Number}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (workflow.WorkflowRuns.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / workflow.WorkflowRuns.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / workflow.WorkflowRuns.Count) * 100}%) output errored");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted {workflow.WorkflowRuns.Count} workflow run(s) for {workflow.Name} workflow in repository {repository}");
        }

        /// <summary>
        /// Outputs the releases to the database.
        /// </summary>
        public async Task OutputReleases(string repository, List<ReleaseModel> releases)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting {releases.Count} release(s) for repository {repository}");

            List<ReleaseModel> successful = [];
            List<ReleaseModel> errored = [];

            foreach (ReleaseModel release in releases)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputting release {release.Id}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\OutputRelease.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = release.Repository },
                        new SqlParameter("@releaseId", System.Data.SqlDbType.BigInt) { Value = release.Id },
                        new SqlParameter("@name", System.Data.SqlDbType.VarChar) { Value = release.Name },
                        new SqlParameter("@author", System.Data.SqlDbType.VarChar) { Value = release.Author.Login },
                        new SqlParameter("@draft", System.Data.SqlDbType.Bit) { Value = release.Draft },
                        new SqlParameter("@assets", System.Data.SqlDbType.Int) { Value = release.Assets.Count },
                        new SqlParameter("@body", System.Data.SqlDbType.VarChar) { Value = release.Body },
                        new SqlParameter("@dateCreated", System.Data.SqlDbType.DateTime) { Value = release.Created_At.UtcDateTime },
                        new SqlParameter("@datePublished", System.Data.SqlDbType.DateTime) { Value = release.Published_At?.UtcDateTime ?? _Clock.DefaultDate },
                    ];

                    (int result, Exception? ex) = _Database.QuerySingle(sql, dataReader =>
                    {
                        return dataReader.GetInt32(0);
                    }, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(release);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output release {release.Id}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 0)
                    {
                        successful.Add(release);

                        _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted release {release.Id}");
                    }
                }

                catch (Exception ex)
                {
                    errored.Add(release);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to output release {release.Id}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (releases.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / releases.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / releases.Count) * 100}%) output errored");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Outputted {releases.Count} release(s) for repository {repository}");
        }

        /// <summary>
        /// Updates or inserts the issue aggregate record in the database.
        /// </summary>
        public async Task LogIssueAggregates(string repository, List<IssueAggregateModel> issueAggregates)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logging {issueAggregates.Count} issue aggregate(s) for repository {repository}");

            List<IssueAggregateModel> successful = [];
            List<IssueAggregateModel> errored = [];

            foreach (IssueAggregateModel issueAggregate in issueAggregates)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logging issue aggregate for {issueAggregate.Date:dd/MM/yyyy HH:mm:ss}");

                try
                {
                    string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\LogIssueAggregate.sql");
                    SqlParameter[] parameters =
                    [
                        new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = repository },
                        new SqlParameter("@date", System.Data.SqlDbType.DateTime) { Value = issueAggregate.Date },
                        new SqlParameter("@created", System.Data.SqlDbType.Int) { Value = issueAggregate.Created },
                        new SqlParameter("@solved", System.Data.SqlDbType.Int) { Value = issueAggregate.Solved }
                    ];

                    (int result, Exception? ex) = _Database.Execute(sql, parameters).Result;

                    if (ex != null)
                    {
                        errored.Add(issueAggregate);

                        _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to log issue aggregate for {issueAggregate.Date:dd/MM/yyyy HH:mm:ss}. Error Message: {ex.Message}");
                        _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                    }

                    if (result == 1)
                    {
                        successful.Add(issueAggregate);

                        _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logged issue aggregate for {issueAggregate.Date:dd/MM/yyyy HH:mm:ss}");
                    }
                }

                catch (Exception ex)
                {
                    errored.Add(issueAggregate);

                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to log issue aggregate for {issueAggregate.Date:dd/MM/yyyy HH:mm:ss}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }
            }

            if (issueAggregates.Count > 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{successful.Count} ({(successful.Count / issueAggregates.Count) * 100}%) output successful, {errored.Count} ({(errored.Count / issueAggregates.Count) * 100}%) output errored");
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logged {issueAggregates.Count} issue aggregate(s) for repository {repository}");
        }

        /// <summary>
        /// Logs the run to the database.
        /// </summary>
        public async Task LogRun(string repository, int issues, int commits, int pullRequests, int workflowRuns, int releases)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logging run for repository {repository}");

            try
            {
                string sql = await _FileSystem.ReadAllText($@"{_Options.SQLFiles}\LogRun.sql");
                SqlParameter[] parameters =
                [
                    new SqlParameter("@repository", System.Data.SqlDbType.VarChar) { Value = repository },
                    new SqlParameter("@issues", System.Data.SqlDbType.Int) { Value = issues },
                    new SqlParameter("@commits", System.Data.SqlDbType.Int) { Value = commits },
                    new SqlParameter("@pullRequests", System.Data.SqlDbType.Int) { Value = pullRequests },
                    new SqlParameter("@workflowRuns", System.Data.SqlDbType.Int) { Value = workflowRuns },
                    new SqlParameter("@releases", System.Data.SqlDbType.Int) { Value = releases }
                ];

                (int result, Exception? ex) = _Database.Execute(sql, parameters).Result;

                if (ex != null)
                {
                    _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to log run for repository {repository}. Error Message: {ex.Message}");
                    _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
                }

                if (result == 1)
                {
                    _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Logged run for repository {repository}");
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Warning, $"Failed to log run for repository {repository}. Error Message: {ex.Message}");
                _Logger.LogMessage(StandardValues.LoggerValues.Error, $"Full Error: {ex}");
            }
        }
    }
}
