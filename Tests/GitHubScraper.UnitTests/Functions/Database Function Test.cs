// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Functions;
using GitHubScraper.Models;
using Moq;

namespace GitHubScraper.UnitTests.Functions
{
    [TestClass]
    public class DatabaseFunctionTest
    {
        #region CreateAggregates

        /// <summary>
        /// Checks whether the CreateAggregates method returns one record for an empty list.
        /// </summary>
        [TestMethod]
        public void TestCreateAggregatesEmpty()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

            List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates("Unit-Test", [], []);

            Assert.AreEqual(1, issueAggregates.Count);
            Assert.IsTrue(issueAggregates[0].Date == date);
            Assert.IsTrue(issueAggregates[0].Created == 0);
            Assert.IsTrue(issueAggregates[0].Solved == 0);
        }

        /// <summary>
        /// Checks whether the CreateAggregates method returns one record for an issue created today.
        /// </summary>
        [TestMethod]
        public void TestCreateAggregatesCreated()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates("Unit-Test", mockIssue, []);

            Assert.AreEqual(1, issueAggregates.Count);
            Assert.IsTrue(issueAggregates[0].Date == date);
            Assert.IsTrue(issueAggregates[0].Created == 1);
            Assert.IsTrue(issueAggregates[0].Solved == 0);
        }

        /// <summary>
        /// Checks whether the CreateAggregates method returns two records for an issue created yesterday and solved today.
        /// </summary>
        [TestMethod]
        public void TestCreateAggregatesCreatedSolved()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

            List<IssueAggregateModel> expected =
            [
                new() { Date = date.AddDays(-1).Date, Created = 1, Solved = 0 },
                new() { Date = date, Created = 0, Solved = 1 }
            ];
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
                    Created_At = date.AddDays(-1),
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            List<IssueAggregateModel> actual = _databaseFunction.CreateAggregates("Unit-Test", mockIssue, []);

            Assert.AreEqual(2, actual.Count);

            if (actual.Count == expected.Count)
            {
                for (int index = 0; index < actual.Count; index++)
                {
                    Assert.AreEqual(expected[index].Date, actual[index].Date);
                    Assert.AreEqual(expected[index].Created, actual[index].Created);
                    Assert.AreEqual(expected[index].Solved, actual[index].Solved);
                }
            }
        }

        /// <summary>
        /// Checks whether the CreateAggregates method fills the gap between the issue dates and now.
        /// </summary>
        [TestMethod]
        public void TestCreateAggregatesFill()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date.AddDays(-15),
                    Closed_At = date.AddDays(-5),
                    Labels = [new() { Name = "bug" }]
                }
            ];

            List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates("Unit-Test", mockIssue, []);

            Assert.AreEqual(16, issueAggregates.Count);

            foreach (IssueAggregateModel issueAggregate in issueAggregates)
            {
                if (issueAggregate.Date == date.AddDays(-15).Date)
                {
                    Assert.IsTrue(issueAggregate.Created == 1);
                    Assert.IsTrue(issueAggregate.Solved == 0);
                }
                else if (issueAggregate.Date == date.AddDays(-5).Date)
                {
                    Assert.IsTrue(issueAggregate.Created == 0);
                    Assert.IsTrue(issueAggregate.Solved == 1);
                }
                else
                {
                    Assert.IsTrue(issueAggregate.Created == 0);
                    Assert.IsTrue(issueAggregate.Solved == 0);
                }
            }
        }

        // /hecks whether the CreateAggregates method returns one record for an existing issue.
        [TestMethod]
        public void TestCreateAggregatesExcludeIssue()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];
            List<IssueModel> mockExistingIssue =
            [
                new()
                {
                    Id = 46578346587688,
                    Number = 0,
                    Title = "UnLoaded",
                    State = "UnLoaded",
                    Created_At = mockIssue[0].Created_At,
                    Closed_At = mockIssue[0].Closed_At,
                    Labels = []
                }
            ];

            List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates("Unit-Test", mockIssue, mockExistingIssue);

            Assert.AreEqual(1, issueAggregates.Count);
            Assert.IsTrue(issueAggregates[0].Date == date.Date);
            Assert.IsTrue(issueAggregates[0].Created == 0);
            Assert.IsTrue(issueAggregates[0].Solved == 0);
        }

        /// <summary>
        /// Checks whether the CreateAggregates method returns one record for an existing issue with no closed date.
        /// </summary>
        [TestMethod]
        public void TestCreateAggregatesExcludeIssueClosed()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];
            List<IssueModel> mockExistingIssue =
            [
                new()
                {
                    Id = 46578346587688,
                    Number = 0,
                    Title = "UnLoaded",
                    State = "UnLoaded",
                    Created_At = mockIssue[0].Created_At,
                    Labels = []
                }
            ];

            List<IssueAggregateModel> issueAggregates = _databaseFunction.CreateAggregates("Unit-Test", mockIssue, mockExistingIssue);

            Assert.AreEqual(1, issueAggregates.Count);
            Assert.IsTrue(issueAggregates[0].Date == date.Date);
            Assert.IsTrue(issueAggregates[0].Created == 0);
            Assert.IsTrue(issueAggregates[0].Solved == 1);
        }

        #endregion

        #region FilterIssues

        /// <summary>
        /// Checks whether the FilterIssues method returns one record for no existing issues.
        /// </summary>
        [TestMethod]
        public void TestFilterIssuesEmpty()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];

            List<IssueModel> filteredIssues = _databaseFunction.FilterIssues("Unit-Test", mockIssue, []);

            Assert.AreEqual(1, filteredIssues.Count);
            Assert.AreEqual(mockIssue[0].Id, filteredIssues[0].Id);
        }

        /// <summary>
        /// Checks whether the FilterIssues method returns no records for an existing issue.
        /// </summary>
        [TestMethod]
        public void TestFilterIssuesExclusion()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];
            List<IssueModel> mockExistingIssue =
            [
                new()
                {
                    Id = 46578346587688,
                    Number = 0,
                    Title = "UnLoaded",
                    State = "UnLoaded",
                    Created_At = mockIssue[0].Created_At,
                    Closed_At = mockIssue[0].Closed_At,
                    Labels = []
                }
            ];

            List<IssueModel> filteredIssues = _databaseFunction.FilterIssues("Unit-Test", mockIssue, mockExistingIssue);

            Assert.AreEqual(0, filteredIssues.Count);
        }

        /// <summary>
        /// Checks whether the FilterIssues method returns one record for an existing issue with no closed date.
        /// </summary>
        [TestMethod]
        public void TestFilterIssuesClosed()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Closed_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];
            List<IssueModel> mockExistingIssue =
            [
                new()
                {
                    Id = 46578346587688,
                    Number = 0,
                    Title = "UnLoaded",
                    State = "UnLoaded",
                    Created_At = _mockClock.Object.DefaultDate,
                    Labels = []
                }
            ];

            List<IssueModel> filteredIssues = _databaseFunction.FilterIssues("Unit-Test", mockIssue, mockExistingIssue);

            Assert.AreEqual(1, filteredIssues.Count);
            Assert.AreEqual(mockIssue[0].Id, filteredIssues[0].Id);
        }

        /// <summary>
        /// Checks whether the FilterIssues method returns no records for an unchanged open issue with the sentinel closed date.
        /// </summary>
        [TestMethod]
        public void TestFilterIssuesOpenUnchanged()
        {
            DateTime date = new(2026, 03, 04, 00, 00, 00, DateTimeKind.Utc);
            DateTime defaultDate = new(1900, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            Mock<ILoggerService> _mockLogger = new();
            Mock<IClock> _mockClock = new();
            _mockClock.Setup(c => c.UtcNow).Returns(date);
            _mockClock.Setup(c => c.DefaultDate).Returns(defaultDate);

            DatabaseFunction _databaseFunction = new(_mockLogger.Object, _mockClock.Object);

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
                    State = "Open",
                    Created_At = date,
                    Labels = [new() { Name = "bug" }]
                }
            ];
            List<IssueModel> mockExistingIssue =
            [
                new()
                {
                    Id = 46578346587688,
                    Number = 0,
                    Title = "UnLoaded",
                    State = "UnLoaded",
                    Created_At = date,
                    Closed_At = defaultDate,
                    Labels = []
                }
            ];

            List<IssueModel> filteredIssues = _databaseFunction.FilterIssues("Unit-Test", mockIssue, mockExistingIssue);

            Assert.AreEqual(0, filteredIssues.Count);
        }

        #endregion
    }
}
