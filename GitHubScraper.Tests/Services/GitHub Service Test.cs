// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using GitHubScraper.Services;
using Moq;

namespace GitHubScraper.Tests.Services
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
                    Assignee = new()
                    {
                        Login = "UnitTester"
                    },
                    Type = "Bug",
                    State = "Closed",
                    Created_At = Date.AddDays(-1),
                    Closed_At = Date,
                    Labels =
                    [
                        new()
                        {
                            Name = "bug"
                        }
                    ]
                }
            ];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssue);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List <IssueModel> issues = await _gitHubService.GetIssues("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.IsTrue(issues.Count > 0);
            Assert.AreEqual(mockIssue[0].Id, issues[0].Id);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns an empty list of issues.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesEmpty()
        {
            List<IssueModel> mockIssue = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetIssues(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockIssue);

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
            List<BranchModel> mockBranch =
            [
                new()
                {
                    Name = "main"
                }
            ];

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
            List<BranchModel> mockBranch = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetBranches(It.IsAny<string>())).ReturnsAsync(mockBranch);

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
                        Author = new()
                        {
                            Name = "UnitTester"
                        },
                        Committer = new()
                        {
                            Name = "UnitTester"
                        },
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
            List<CommitModel> mockCommit = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetCommits(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(mockCommit);

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
                    Assignee = new()
                    {
                        Login = "UnitTester"
                    },
                    Type = "Bug",
                    State = "Closed",
                    Created_At = Date.AddDays(-1),
                    Updated_At = Date,
                    Closed_At = Date,
                    Merged_At = Date,
                    Labels =
                    [
                        new()
                        {
                            Name = "bug"
                        }
                    ]
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
            List<PullRequestModel> mockPullRequest = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetPullRequests(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockPullRequest);

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
                    Actor = new()
                    {
                        Login = "UnitTester"
                    },
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
            List<WorkflowRunModel> mockWorkflowRun = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetWorkflowRuns(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockWorkflowRun);

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
                    Author = new()
                    {
                        Login = "UnitTester"
                    },
                    Body = "This is a test release.",
                    Draft = false,
                    Created_At = Date,
                    Updated_At = Date,
                    Published_At = Date.AddDays(1),
                    Assets =
                    [
                        new()
                        {
                            Id = 465783465
                        }
                    ]
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
            List<ReleaseModel> mockRelease = [];

            Mock<IGitHubClient> _mockGitHubClient = new();
            _mockGitHubClient.Setup(ghc => ghc.GetReleases(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(mockRelease);

            GitHubService _gitHubService = new(_MockLogger.Object, _mockGitHubClient.Object);

            List<ReleaseModel> releases = await _gitHubService.GetReleases("Unit-Test", _MockClock.Object.DefaultDate);

            Assert.AreEqual(0, releases.Count);
        }
    }
}
