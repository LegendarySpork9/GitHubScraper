// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using GitHubScraper.Services;
using Moq;

namespace GitHubScraper.IntegrationTests.Services
{
    [TestClass]
    public class GitHubServiceTest
    {
        private DateTime Date;
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();

        /// <summary>
        /// Sets the mocks up for the tests.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Date = new(2026, 03, 05, 00, 00, 00, DateTimeKind.Utc);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns a list of issues.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssues()
        {
            List<IssueModel> mockIssue =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Id = 46578346587688,
                    Number = 1,
                    Title = "Test",
                    Assignee = new() { Login = "UnitTester" },
                    Type = "Bug",
                    State = "Closed",
                    Created_At = Date.AddDays(-1),
                    Closed_At = Date,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssue);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.IsTrue(issues.Count > 0);
            Assert.AreEqual(mockIssue[0].Id, issues[0].Id);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns an empty list of issues.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", Date);

            Assert.AreEqual(0, issues.Count);
        }

        /// <summary>
        /// Checks whether the GetBranches method returns a list of branches.
        /// </summary>
        [TestMethod]
        public async Task TestGetBranches()
        {
            List<BranchModel> mockBranch = [new() { Name = "main" }];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetBranches(It.IsAny<string>())).ReturnsAsync(mockBranch);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<BranchModel> branches = await _gitHubService.GetBranches("Unit-Test");

            Assert.IsTrue(branches.Count > 0);
            Assert.AreEqual(mockBranch[0].Name, branches[0].Name);
        }

        /// <summary>
        /// Checks whether the GetBranches method returns an empty list of branches.
        /// </summary>
        [TestMethod]
        public async Task TestGetBranchesEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetBranches(It.IsAny<string>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<BranchModel> branches = await _gitHubService.GetBranches("Unit-Test");

            Assert.AreEqual(0, branches.Count);
        }

        /// <summary>
        /// Checks whether the GetCommits method returns a list of commits.
        /// </summary>
        [TestMethod]
        public async Task TestGetCommits()
        {
            List<CommitModel> mockCommit =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Sha = "g5vyb65yg6ybhgbhvfgh665664637yvtvt",
                    Commit = new()
                    {
                        Author = new() { Name = "UnitTester" },
                        Committer = new() { Name = "UnitTester" },
                        Message = "This is a test message."
                    }
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetCommits(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(mockCommit);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<CommitModel> commits = await _gitHubService.GetCommits("Unit-Test", _MockClock.Object.DefaultDate, "main");

            Assert.IsTrue(commits.Count > 0);
            Assert.AreEqual(mockCommit[0].Sha, commits[0].Sha);
        }

        /// <summary>
        /// Checks whether the GetCommits method returns an empty list of commits.
        /// </summary>
        [TestMethod]
        public async Task TestGetCommitsEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetCommits(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<CommitModel> commits = await _gitHubService.GetCommits("Unit-Test", Date, "main");

            Assert.AreEqual(0, commits.Count);
        }

        /// <summary>
        /// Checks whether the GetPullRequests method returns a list of pull requests.
        /// </summary>
        [TestMethod]
        public async Task TestGetPullRequests()
        {
            List<PullRequestModel> mockPullRequest =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Id = 46578346587688,
                    Number = 1,
                    Title = "Test",
                    Assignee = new() { Login = "UnitTester" },
                    Type = "Bug",
                    State = "Closed",
                    Created_At = Date.AddDays(-1),
                    Updated_At = Date,
                    Closed_At = Date,
                    Merged_At = Date,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetPullRequests(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockPullRequest);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<PullRequestModel> pullRequests = await _gitHubService.GetPullRequests("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.IsTrue(pullRequests.Count > 0);
            Assert.AreEqual(mockPullRequest[0].Id, pullRequests[0].Id);
        }

        /// <summary>
        /// Checks whether the GetPullRequests method returns an empty list of pull requests.
        /// </summary>
        [TestMethod]
        public async Task TestGetPullRequestsEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetPullRequests(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<PullRequestModel> pullRequests = await _gitHubService.GetPullRequests("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.AreEqual(0, pullRequests.Count);
        }

        /// <summary>
        /// Checks whether the GetWorkflowRuns method returns a list of workflow runs.
        /// </summary>
        [TestMethod]
        public async Task TestGetWorkflowRuns()
        {
            List<WorkflowRunModel> mockWorkflowRun =
            [
                new()
                {
                    Id = 46578346587688,
                    Run_Number = 1,
                    Actor = new() { Login = "UnitTester" },
                    Name = "test_workflow",
                    Display_Title = "Test",
                    Event = "pull_request",
                    Status = "completed",
                    Conclusion = "success",
                    Created_At = Date,
                    Updated_At = Date
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetWorkflowRuns(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockWorkflowRun);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<WorkflowRunModel> workflowRuns = await _gitHubService.GetWorkflowRuns("Unit-Test", "Test Workflow", _MockClock.Object.DefaultDate);

            Assert.IsTrue(workflowRuns.Count > 0);
            Assert.AreEqual(mockWorkflowRun[0].Id, workflowRuns[0].Id);
        }

        /// <summary>
        /// Checks whether the GetWorkflowRuns method returns an empty list of workflow runs.
        /// </summary>
        [TestMethod]
        public async Task TestGetWorkflowRunsEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetWorkflowRuns(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<WorkflowRunModel> workflowRuns = await _gitHubService.GetWorkflowRuns("Unit-Test", "Test Workflow", _MockClock.Object.DefaultDate);

            Assert.AreEqual(0, workflowRuns.Count);
        }

        /// <summary>
        /// Checks whether the GetReleases method returns a list of releases.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleases()
        {
            List<ReleaseModel> mockRelease =
            [
                new()
                {
                    Id = 46578346587688,
                    Name = "Test",
                    Author = new() { Login = "UnitTester" },
                    Body = "This is a test release.",
                    Draft = false,
                    Created_At = Date,
                    Updated_At = Date,
                    Published_At = Date.AddDays(1),
                    Assets = [new() { Id = 465783465 }]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetReleases(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockRelease);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<ReleaseModel> releases = await _gitHubService.GetReleases("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.IsTrue(releases.Count > 0);
            Assert.AreEqual(mockRelease[0].Id, releases[0].Id);
        }

        /// <summary>
        /// Checks whether the GetReleases method returns an empty list of releases.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesEmpty()
        {
            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetReleases(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync([]);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<ReleaseModel> releases = await _gitHubService.GetReleases("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.AreEqual(0, releases.Count);
        }

        /// <summary>
        /// Checks whether the GetIssues method filters out pull requests.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesFiltersPullRequests()
        {
            List<IssueModel> mockIssues =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Id = 1,
                    Number = 1,
                    Title = "Real Issue",
                    State = "open",
                    Created_At = Date,
                    Labels = []
                },
                new()
                {
                    Repository = "Unit-Test",
                    Id = 2,
                    Number = 2,
                    Title = "Actually a PR",
                    State = "open",
                    Pull_Request = new object(),
                    Created_At = Date,
                    Labels = []
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssues);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", Date);

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("Real Issue", issues[0].Title);
        }

        /// <summary>
        /// Checks whether the GetIssues method extracts the type from labels.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesExtractsTypeFromLabels()
        {
            List<IssueModel> mockIssues =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Id = 1,
                    Number = 1,
                    Title = "Bug Report",
                    State = "open",
                    Created_At = Date,
                    Labels = [new() { Name = "bug" }, new() { Name = "priority" }]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssues);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", Date);

            Assert.AreEqual("Bug", issues[0].Type);
        }

        /// <summary>
        /// Checks whether the GetIssues method title-cases the state.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesTitleCasesState()
        {
            List<IssueModel> mockIssues =
            [
                new()
                {
                    Repository = "Unit-Test",
                    Id = 1,
                    Number = 1,
                    Title = "Test",
                    State = "closed",
                    Created_At = Date,
                    Closed_At = Date,
                    Labels = []
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssues);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", Date);

            Assert.AreEqual("Closed", issues[0].State);
        }

        /// <summary>
        /// Checks whether the GetIssues method sets the repository on each issue.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesSetsRepository()
        {
            List<IssueModel> mockIssues =
            [
                new()
                {
                    Id = 1,
                    Number = 1,
                    Title = "Test",
                    State = "open",
                    Created_At = Date,
                    Labels = []
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssues);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<IssueModel> issues = await _gitHubService.GetIssues("My-Repo", Date);

            Assert.AreEqual("My-Repo", issues[0].Repository);
        }

        /// <summary>
        /// Checks whether the GetWorkflowRuns method replaces underscores and title-cases the event.
        /// </summary>
        [TestMethod]
        public async Task TestGetWorkflowRunsNormalisesEvent()
        {
            List<WorkflowRunModel> mockRuns =
            [
                new()
                {
                    Id = 1,
                    Run_Number = 1,
                    Actor = new() { Login = "Tester" },
                    Name = "CI",
                    Display_Title = "Test",
                    Event = "pull_request",
                    Status = "completed",
                    Conclusion = "success",
                    Created_At = Date,
                    Updated_At = Date
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetWorkflowRuns(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockRuns);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<WorkflowRunModel> runs = await _gitHubService.GetWorkflowRuns("Unit-Test", "CI", Date);

            Assert.AreEqual("Pull Request", runs[0].Event);
            Assert.AreEqual("Completed", runs[0].Status);
            Assert.AreEqual("Success", runs[0].Conclusion);
        }

        /// <summary>
        /// Checks whether the GetReleases method sets the NumberOfAssets from the Assets list.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesSetsAssetCount()
        {
            List<ReleaseModel> mockReleases =
            [
                new()
                {
                    Id = 1,
                    Name = "v1.0",
                    Author = new() { Login = "Tester" },
                    Body = "Release notes",
                    Draft = false,
                    Created_At = Date,
                    Updated_At = Date,
                    Assets = [new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 }]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetReleases(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockReleases);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<ReleaseModel> releases = await _gitHubService.GetReleases("Unit-Test", Date);

            Assert.AreEqual(3, releases[0].NumberOfAssets);
        }

        /// <summary>
        /// Checks whether the GetCommits method sets the repository on each commit.
        /// </summary>
        [TestMethod]
        public async Task TestGetCommitsSetsRepository()
        {
            List<CommitModel> mockCommits =
            [
                new()
                {
                    Sha = "abc123",
                    Commit = new()
                    {
                        Author = new() { Name = "Author" },
                        Committer = new() { Name = "Committer", Date = Date },
                        Message = "Test"
                    }
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetCommits(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(mockCommits);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<CommitModel> commits = await _gitHubService.GetCommits("My-Repo", Date, "main");

            Assert.AreEqual("My-Repo", commits[0].Repository);
        }

        /// <summary>
        /// Checks whether the GetPullRequests method handles null Merged_At correctly.
        /// </summary>
        [TestMethod]
        public async Task TestGetPullRequestsHandlesNullMergedAt()
        {
            List<PullRequestModel> mockPRs =
            [
                new()
                {
                    Id = 1,
                    Number = 1,
                    Title = "Open PR",
                    Assignee = new() { Login = "Tester" },
                    State = "open",
                    Created_At = Date,
                    Updated_At = Date,
                    Labels = []
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetPullRequests(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockPRs);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<PullRequestModel> prs = await _gitHubService.GetPullRequests("Unit-Test", Date);

            Assert.AreEqual(1, prs.Count);
            Assert.IsNull(prs[0].Merged_At);
            Assert.IsNull(prs[0].Closed_At);
            Assert.AreEqual("Open", prs[0].State);
        }
    }
}
