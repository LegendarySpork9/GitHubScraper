// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Models;

namespace GitHubScraper.Functions
{
    public class DatabaseFunction
    {
        private readonly ILoggerService _Logger;
        private readonly IClock _Clock;

        // Sets the class's global variables.
        public DatabaseFunction(
            ILoggerService _logger,
            IClock _clock)
        {
            _Logger = _logger;
            _Clock = _clock;
        }

        /// <summary>
        /// Creates the issue aggregates.
        /// </summary>
        public List<IssueAggregateModel> CreateAggregates(string repository, List<IssueModel> issues, List<IssueModel> existingIssues)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Creating aggregates for repository {repository}");

            Dictionary<DateTime, IssueAggregateModel> issueAggregatesDictionary = [];
            Dictionary<long, IssueModel> existingIssuesDictionary = existingIssues.ToDictionary(i => i.Id);

            foreach (IssueModel issue in issues)
            {
                existingIssuesDictionary.TryGetValue(issue.Id, out IssueModel? existingIssue);

                if (existingIssue == null || issue.Created_At != existingIssue.Created_At)
                {
                    DateTime createdDate = issue.Created_At.Date;

                    if (!issueAggregatesDictionary.TryGetValue(createdDate, out IssueAggregateModel? issueAggregate))
                    {
                        issueAggregate = new IssueAggregateModel
                        {
                            Date = createdDate
                        };

                        issueAggregatesDictionary[createdDate] = issueAggregate;
                    }

                    issueAggregate.Created++;
                }

                if (issue.Closed_At is DateTimeOffset && (existingIssue == null || issue.Closed_At != existingIssue.Closed_At))
                {
                    DateTime closedDate = issue.Closed_At.Value.Date;

                    if (!issueAggregatesDictionary.TryGetValue(closedDate, out IssueAggregateModel? issueAggregate))
                    {
                        issueAggregate = new IssueAggregateModel
                        {
                            Date = closedDate
                        };

                        issueAggregatesDictionary[closedDate] = issueAggregate;
                    }

                    issueAggregate.Solved++;
                }
            }

            List<IssueAggregateModel> sortedIssueAggregates = [.. issueAggregatesDictionary.Values.OrderBy(iad => iad.Date)];

            if (sortedIssueAggregates.Count == 0)
            {
                _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"1 aggregate created");
                _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Created aggregates for repository {repository}");

                return [new IssueAggregateModel
                {
                    Date = _Clock.UtcNow.Date,
                    Created = 0,
                    Solved = 0
                }];
            }

            List<IssueAggregateModel> issueAggregates = [];
            DateTime start = sortedIssueAggregates.First().Date;
            DateTime end = _Clock.UtcNow.Date;

            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (issueAggregatesDictionary.TryGetValue(date, out IssueAggregateModel? issueAggregate))
                {
                    issueAggregates.Add(issueAggregate);
                }

                else
                {
                    issueAggregates.Add(new IssueAggregateModel
                    {
                        Date = date,
                        Created = 0,
                        Solved = 0
                    });
                }
            }

            _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{issueAggregates.Count} aggregate(s) created");
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Created aggregates for repository {repository}");
            return issueAggregates;
        }

        /// <summary>
        /// Filters out existing issues from the given list of issues.
        /// </summary>
        public List<IssueModel> FilterIssues(string repository, List<IssueModel> issues, List<IssueModel> existingIssues)
        {
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filtering issues for repository {repository}");

            Dictionary<long, IssueModel> existingIssuesDictionary = existingIssues.ToDictionary(i => i.Id);

            int removedIssues = issues.RemoveAll(i =>
            {
                if (!existingIssuesDictionary.TryGetValue(i.Id, out IssueModel? existingIssue))
                {
                    return false;
                }

                return i.Created_At == existingIssue.Created_At && i.Closed_At == existingIssue.Closed_At;
            });

            _Logger.LogMessage(StandardValues.LoggerValues.Debug, $"{removedIssues} issue(s) removed");
            _Logger.LogMessage(StandardValues.LoggerValues.Info, $"Filtered issues for repository {repository}");
            return issues;
        }
    }
}
