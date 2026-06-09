// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Models;
using GitHubScraper.Services;
using Microsoft.Data.SqlClient;
using Moq;

namespace GitHubScraper.Tests.Services
{
    [TestClass]
    public class DatabaseServiceTest
    {
        private DateTime Date;
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();
        private readonly Mock<IDatabaseOptions> _MockOptions = new();

        /// <summary>
        /// Sets the mocks up for the tests.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Date = new(2026, 03, 05, 00, 00, 00, DateTimeKind.Utc);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).ReturnsAsync("select 1");
            _MockOptions.Setup(o => o.ConnectionString).Returns("This is a connection string");
            _MockOptions.Setup(o => o.SQLFiles).Returns(@"C:\SQL");
        }

        /// <summary>
        /// Checks whether the GetLastRunDate method returns 01/01/1900 if the given repository has never run before.
        /// </summary>
        [TestMethod]
        public async Task TestGetLastRunDateNew()
        {
            DateTime expected = _MockClock.Object.DefaultDate;

            Mock<IDatabase> _mockDatabase = new();
            _mockDatabase.Setup(d => d.QuerySingle(It.IsAny<string>(), It.IsAny<Func<SqlDataReader, DateTime>>(), It.IsAny<SqlParameter[]>()).Result).Returns((expected, null));

            DatabaseService _databaseService = new(_MockLogger.Object, _MockClock.Object, _MockFileSystem.Object, _MockOptions.Object, _mockDatabase.Object);
            DateTime actual = await _databaseService.GetLastRunDate("Unit-Test");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Checks whether the GetLastRunDate method returns the last time the given repository ran.
        /// </summary>
        [TestMethod]
        public async Task TestGetLastRunDate()
        {
            Mock<IDatabase> _mockDatabase = new();
            _mockDatabase.Setup(d => d.QuerySingle(It.IsAny<string>(), It.IsAny<Func<SqlDataReader, DateTime>>(), It.IsAny<SqlParameter[]>()).Result).Returns((Date, null));

            DatabaseService _databaseService = new(_MockLogger.Object, _MockClock.Object, _MockFileSystem.Object, _MockOptions.Object, _mockDatabase.Object);
            DateTime actual = await _databaseService.GetLastRunDate("Unit-Test");
            
            Assert.AreEqual(Date, actual);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns an empty list if there are no issues for the given repository.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssuesNone()
        {
            Mock<IDatabase> _mockDatabase = new();
            _mockDatabase.Setup(d => d.Query(It.IsAny<string>(), It.IsAny<Func<SqlDataReader, IssueModel>>(), It.IsAny<SqlParameter[]>()).Result).Returns(([], null));

            DatabaseService _databaseService = new(_MockLogger.Object, _MockClock.Object, _MockFileSystem.Object, _MockOptions.Object, _mockDatabase.Object);
            List<IssueModel> issues = await _databaseService.GetIssues("Unit-Test");

            Assert.AreEqual(0, issues.Count);
        }

        /// <summary>
        /// Checks whether the GetIssues method returns an list of issues if there are some issues for the given repository.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssues()
        {
            IssueModel issue = new()
            {
                Id = 46578346587688,
                Number = 0,
                Title = "UnLoaded",
                State = "UnLoaded",
                Created_At = Date,
                Closed_At = Date,
                Labels = []
            };

            Mock<IDatabase> _mockDatabase = new();
            _mockDatabase.Setup(d => d.Query(It.IsAny<string>(), It.IsAny<Func<SqlDataReader, IssueModel>>(), It.IsAny<SqlParameter[]>()).Result).Returns(([issue], null));

            DatabaseService _databaseService = new(_MockLogger.Object, _MockClock.Object, _MockFileSystem.Object, _MockOptions.Object, _mockDatabase.Object);

            List<IssueModel> expected = [issue];
            List<IssueModel> actual = await _databaseService.GetIssues("Unit-Test");

            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0].Id, actual[0].Id);
        }
    }
}
