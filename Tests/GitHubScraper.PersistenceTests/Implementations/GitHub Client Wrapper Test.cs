// Copyright © - Unpublished - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Implementations;
using GitHubScraper.Models;
using GitHubScraper.Models.Related;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace GitHubScraper.PersistenceTests.Implementations
{
    [TestClass]
    public class GitHubClientWrapperTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();

        private Mock<IGitHubOptions> CreateMockOptions()
        {
            Mock<IGitHubOptions> mock = new();
            mock.Setup(o => o.Owner).Returns("TestOwner");
            mock.Setup(o => o.BearerToken).Returns("test-token");

            return mock;
        }

        private Mock<IRestClientWrapper> CreateMockRestClient(
            HttpStatusCode statusCode,
            string? content)
        {
            Mock<IRestClientWrapper> mock = new();
            RestResponse response = new()
            {
                StatusCode = statusCode,
                Content = content
            };
            mock.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(response);

            return mock;
        }

        /// <summary>
        /// Checks whether the GetIssues method returns the issue list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssues()
        {
            List<IssueModel> expected =
            [
                new()
                {
                    Id = 1,
                    Number = 1,
                    Title = "Test Issue",
                    State = "open",
                    Created_At = DateTimeOffset.UtcNow,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            string responseJson = JsonConvert.SerializeObject(expected);
            Mock<IRestClientWrapper> _mockRestClient = CreateMockRestClient(
                HttpStatusCode.OK,
                responseJson);

            int callCount = 0;
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : "[]"
                    };
                });

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<IssueModel> actual = await _wrapper.GetIssues("TestRepo", DateTime.UtcNow.AddDays(-7));

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "Test Issue",
                actual[0].Title);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns an empty list on error.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesError()
        {
            Mock<IRestClientWrapper> _mockRestClient = CreateMockRestClient(
                HttpStatusCode.Unauthorized,
                null);

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<IssueModel> actual = await _wrapper.GetIssues("TestRepo", DateTime.UtcNow.AddDays(-7));

            Assert.AreEqual(
                0,
                actual.Count);
        }

        /// <summary>
        /// Checks whether the GetBranches method returns the branch list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetBranches()
        {
            List<BranchModel> expected = [new() { Name = "main" }];

            string responseJson = JsonConvert.SerializeObject(expected);

            int callCount = 0;
            Mock<IRestClientWrapper> _mockRestClient = new();
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : "[]"
                    };
                });

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<BranchModel> actual = await _wrapper.GetBranches("TestRepo");

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "main",
                actual[0].Name);
        }

        /// <summary>
        /// Checks whether the GetCommits method returns the commit list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetCommits()
        {
            List<CommitModel> expected =
            [
                new()
                {
                    Sha = "abc123",
                    Commit = new()
                    {
                        Author = new() { Name = "Tester" },
                        Committer = new() { Name = "Tester" },
                        Message = "Test commit"
                    }
                }
            ];

            string responseJson = JsonConvert.SerializeObject(expected);

            int callCount = 0;
            Mock<IRestClientWrapper> _mockRestClient = new();
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : "[]"
                    };
                });

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<CommitModel> actual = await _wrapper.GetCommits("TestRepo", DateTime.UtcNow.AddDays(-7), "main");

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "abc123",
                actual[0].Sha);
        }

        /// <summary>
        /// Checks whether the GetPullRequests method returns the pull request list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetPullRequests()
        {
            DateTime lastRunDate = DateTime.UtcNow.AddDays(-7);

            List<PullRequestModel> expected =
            [
                new()
                {
                    Id = 1,
                    Number = 1,
                    Title = "Test PR",
                    Assignee = new() { Login = "Tester" },
                    State = "open",
                    Created_At = DateTimeOffset.UtcNow.AddDays(-1),
                    Updated_At = DateTimeOffset.UtcNow,
                    Labels = []
                }
            ];

            string responseJson = JsonConvert.SerializeObject(expected);

            int callCount = 0;
            Mock<IRestClientWrapper> _mockRestClient = new();
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : "[]"
                    };
                });

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<PullRequestModel> actual = await _wrapper.GetPullRequests("TestRepo", lastRunDate);

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "Test PR",
                actual[0].Title);
        }

        /// <summary>
        /// Checks whether the GetWorkflowRuns method returns the workflow run list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetWorkflowRuns()
        {
            DateTime lastRunDate = DateTime.UtcNow.AddDays(-7);

            var responseObject = new
            {
                total_count = 1,
                workflow_runs = new[]
                {
                    new
                    {
                        id = 100L,
                        run_number = 1,
                        actor = new { login = "Tester" },
                        name = "CI",
                        display_title = "Test Run",
                        @event = "push",
                        status = "completed",
                        conclusion = "success",
                        created_at = DateTimeOffset.UtcNow.AddDays(-1),
                        updated_at = DateTimeOffset.UtcNow
                    }
                }
            };

            string responseJson = JsonConvert.SerializeObject(responseObject);
            string emptyJson = JsonConvert.SerializeObject(new { total_count = 0 });

            int callCount = 0;
            Mock<IRestClientWrapper> _mockRestClient = new();
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : emptyJson
                    };
                });

            _MockClock.Setup(c => c.DefaultDate).Returns(new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc));
            _MockClock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<WorkflowRunModel> actual = await _wrapper.GetWorkflowRuns("TestRepo", "CI.yml", lastRunDate);

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "Test Run",
                actual[0].Display_Title);
        }

        /// <summary>
        /// Checks whether the GetReleases method returns the release list on success.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleases()
        {
            DateTime lastRunDate = DateTime.UtcNow.AddDays(-7);

            List<ReleaseModel> expected =
            [
                new()
                {
                    Id = 1,
                    Name = "v1.0.0",
                    Author = new() { Login = "Tester" },
                    Body = "First release",
                    Draft = false,
                    Created_At = DateTimeOffset.UtcNow.AddDays(-1),
                    Updated_At = DateTimeOffset.UtcNow,
                    Assets = [new() { Id = 1 }]
                }
            ];

            string responseJson = JsonConvert.SerializeObject(expected);

            int callCount = 0;
            Mock<IRestClientWrapper> _mockRestClient = new();
            _mockRestClient.Setup(rc => rc.ExecuteAsync(It.IsAny<string>(), It.IsAny<RestRequest>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return new RestResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = callCount == 1 ? responseJson : "[]"
                    };
                });

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            List<ReleaseModel> actual = await _wrapper.GetReleases("TestRepo", lastRunDate);

            Assert.AreEqual(
                1,
                actual.Count);
            Assert.AreEqual(
                "v1.0.0",
                actual[0].Name);
        }

        /// <summary>
        /// Checks whether the GetIssues method includes the Bearer token in requests.
        /// </summary>
        [TestMethod]
        public async Task TestBearerTokenIncluded()
        {
            Mock<IRestClientWrapper> _mockRestClient = CreateMockRestClient(
                HttpStatusCode.OK,
                "[]");

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            await _wrapper.GetIssues("TestRepo", DateTime.UtcNow);

            _mockRestClient.Verify(
                rc => rc.ExecuteAsync(
                    It.IsAny<string>(),
                    It.Is<RestRequest>(r => r.Parameters.Any(
                        p => p.Name == "Authorization" && p.Value != null && p.Value.ToString()!.Contains("test-token")))),
                Times.Once);
        }

        /// <summary>
        /// Checks whether the URL is constructed correctly with owner and repository.
        /// </summary>
        [TestMethod]
        public async Task TestURLConstruction()
        {
            Mock<IRestClientWrapper> _mockRestClient = CreateMockRestClient(
                HttpStatusCode.OK,
                "[]");

            Mock<IGitHubOptions> _mockOptions = CreateMockOptions();

            GitHubClientWrapper _wrapper = new(
                _MockLogger.Object,
                _mockOptions.Object,
                _MockClock.Object,
                _mockRestClient.Object);

            await _wrapper.GetBranches("MyRepo");

            _mockRestClient.Verify(
                rc => rc.ExecuteAsync(
                    It.Is<string>(url => url.Contains("TestOwner/MyRepo/branches")),
                    It.IsAny<RestRequest>()),
                Times.AtLeastOnce);
        }
    }
}
