// Copyright © - 31/08/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Implementations;
using GitHubScraper.Models;
using GitHubScraper.Services;
using Microsoft.Data.SqlClient;
using Moq;

namespace GitHubScraper.PersistenceTests.Services
{
    [TestClass]
    public class DatabaseServiceTest
    {
        private static string _DatabaseName = null!;
        private static string _ConnectionString = null!;
        private static string _SqlFilesPath = null!;
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();

        /// <summary>
        /// Creates a LocalDB test database with the full schema and stored procedures.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _DatabaseName = $"GitHubScraper_Test_{Guid.NewGuid():N}";
            string masterConnection = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;";
            _ConnectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={_DatabaseName};Integrated Security=true;";

            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            _SqlFilesPath = Path.Combine(projectRoot, "GitHubScraper", "SQL");

            using (SqlConnection connection = new(masterConnection))
            {
                connection.Open();

                using (SqlCommand command = new($"CREATE DATABASE [{_DatabaseName}]", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                string[] tableStatements =
                [
                    "CREATE TABLE [dbo].[Repository]([RepositoryId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] VARCHAR(50) NOT NULL)",
                    "CREATE TABLE [dbo].[User]([UserId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Username] VARCHAR(50) NOT NULL)",
                    "CREATE TABLE [dbo].[Type]([TypeId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Value] VARCHAR(20) NOT NULL)",
                    "CREATE TABLE [dbo].[Status]([StatusId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Value] VARCHAR(20) NOT NULL)",
                    "CREATE TABLE [dbo].[Event]([EventId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] VARCHAR(255) NOT NULL)",
                    "CREATE TABLE [dbo].[Workflow]([WorkflowId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] VARCHAR(255) NOT NULL)",
                    "CREATE TABLE [dbo].[Issue]([IssueId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [RepositoryId] INT NOT NULL, [AssigneeId] INT NOT NULL, [TypeId] INT NOT NULL, [StatusId] INT NOT NULL, [GitHubIssueId] BIGINT NOT NULL, [Title] VARCHAR(80) NOT NULL, [Number] INT NOT NULL, [DateCreated] DATETIME NOT NULL, [DateSolved] DATETIME NOT NULL DEFAULT('1900-01-01'))",
                    "CREATE TABLE [dbo].[Commit]([CommitId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [RepositoryId] INT NOT NULL, [AuthorId] INT NOT NULL, [CommitterId] INT NOT NULL, [GitHubCommitId] VARCHAR(50) NOT NULL, [Message] VARCHAR(MAX) NOT NULL)",
                    "CREATE TABLE [dbo].[RunHistory]([RunHistoryId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [RepositoryId] INT NOT NULL, [RunDate] DATETIME NOT NULL, [Issues] INT NOT NULL, [Commits] INT NOT NULL, [PullRequests] INT NOT NULL, [WorkflowRuns] INT NOT NULL, [Releases] INT NOT NULL)",
                    "CREATE TABLE [dbo].[IssueAggregate]([IssueAggregateId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [RepositoryId] INT NOT NULL, [Date] DATETIME NOT NULL, [Created] INT NOT NULL, [Solved] INT NOT NULL)"
                ];

                foreach (string sql in tableStatements)
                {
                    using (SqlCommand command = new(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                string storedProcSql = @"
                    CREATE PROCEDURE [dbo].[StoreIssue]
                        @repository varchar(50), @issueId bigint, @number int, @title varchar(80),
                        @assignee varchar(50), @type varchar(20), @status varchar(20),
                        @dateCreated datetime, @dateSolved datetime = '1900-01-01 00:00:00.000'
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        DECLARE @repositoryId int, @assigneeId int, @typeId int, @statusId int

                        MERGE Repository AS [target] USING (SELECT @repository AS Repo) AS [source] ON [target].[Name] = [source].Repo
                        WHEN NOT MATCHED THEN INSERT ([Name]) VALUES ([source].Repo);
                        SELECT @repositoryId = RepositoryId FROM Repository WITH (NOLOCK) WHERE [Name] = @repository

                        MERGE [User] AS [target] USING (SELECT @assignee AS Username) AS [source] ON [target].Username = [source].Username
                        WHEN NOT MATCHED THEN INSERT (Username) VALUES ([source].Username);
                        SELECT @assigneeId = UserId FROM [User] WITH (NOLOCK) WHERE Username = @assignee

                        MERGE [Type] AS [target] USING (SELECT @type AS IssueType) AS [source] ON [target].[Value] = [source].IssueType
                        WHEN NOT MATCHED THEN INSERT ([Value]) VALUES ([source].IssueType);
                        SELECT @typeId = TypeId FROM [Type] WITH (NOLOCK) WHERE [Value] = @type

                        MERGE [Status] AS [target] USING (SELECT @status AS IssueStatus) AS [source] ON [target].[Value] = [source].IssueStatus
                        WHEN NOT MATCHED THEN INSERT ([Value]) VALUES ([source].IssueStatus);
                        SELECT @statusId = StatusId FROM [Status] WITH (NOLOCK) WHERE [Value] = @status

                        MERGE Issue AS [target]
                        USING (SELECT @repositoryId AS RepositoryId, @issueId AS GitHubIssueId, @number AS Number, @title AS Title,
                            @assigneeId AS AssigneeId, @typeId AS TypeId, @statusId AS StatusId, @dateCreated AS DateCreated, @dateSolved AS DateSolved) AS [source]
                        ON [target].GitHubIssueId = [source].GitHubIssueId
                        WHEN MATCHED THEN UPDATE SET Title = [source].Title, AssigneeId = [source].AssigneeId, TypeId = [source].TypeId, StatusId = [source].StatusId, DateSolved = [source].DateSolved
                        WHEN NOT MATCHED THEN INSERT (RepositoryId, GitHubIssueId, Number, Title, AssigneeId, TypeId, StatusId, DateCreated, DateSolved)
                            VALUES ([source].RepositoryId, [source].GitHubIssueId, [source].Number, [source].Title, [source].AssigneeId, [source].TypeId, [source].StatusId, [source].DateCreated, [source].DateSolved);
                    END";

                using (SqlCommand command = new(storedProcSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                string storeCommitSql = @"
                    CREATE PROCEDURE [dbo].[StoreCommit]
                        @repository varchar(50), @author varchar(50), @committer varchar(50), @sha varchar(50), @message varchar(max)
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        DECLARE @repositoryId int, @authorId int, @committerId int

                        MERGE Repository AS [target] USING (SELECT @repository AS Repo) AS [source] ON [target].[Name] = [source].Repo
                        WHEN NOT MATCHED THEN INSERT ([Name]) VALUES ([source].Repo);
                        SELECT @repositoryId = RepositoryId FROM Repository WITH (NOLOCK) WHERE [Name] = @repository

                        MERGE [User] AS [target] USING (SELECT @author AS Username) AS [source] ON [target].Username = [source].Username
                        WHEN NOT MATCHED THEN INSERT (Username) VALUES ([source].Username);
                        SELECT @authorId = UserId FROM [User] WITH (NOLOCK) WHERE Username = @author

                        MERGE [User] AS [target] USING (SELECT @committer AS Username) AS [source] ON [target].Username = [source].Username
                        WHEN NOT MATCHED THEN INSERT (Username) VALUES ([source].Username);
                        SELECT @committerId = UserId FROM [User] WITH (NOLOCK) WHERE Username = @committer

                        MERGE [Commit] AS [target]
                        USING (SELECT @repositoryId AS RepositoryId, @sha AS GitHubCommitId, @authorId AS AuthorId, @committerId AS CommitterId, @message AS [Message]) AS [source]
                        ON [target].GitHubCommitId = [source].GitHubCommitId
                        WHEN MATCHED THEN UPDATE SET AuthorId = [source].AuthorId, CommitterId = [source].CommitterId, [Message] = [source].[Message]
                        WHEN NOT MATCHED THEN INSERT (RepositoryId, AuthorId, CommitterId, GitHubCommitId, [Message])
                            VALUES ([source].RepositoryId, [source].AuthorId, [source].CommitterId, [source].GitHubCommitId, [source].[Message]);
                    END";

                using (SqlCommand command = new(storeCommitSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Drops the test database.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            string masterConnection = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;";

            using (SqlConnection connection = new(masterConnection))
            {
                connection.Open();

                using (SqlCommand command = new(
                    $"ALTER DATABASE [{_DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_DatabaseName}]", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sets up the clock mock.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _MockClock.Setup(c => c.DefaultDate).Returns(new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <summary>
        /// Checks whether the GetLastRunDate method returns the default date for a new repository.
        /// </summary>
        [TestMethod]
        public async Task TestGetLastRunDateNew()
        {
            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            DateTime result = await _service.GetLastRunDate("NonExistentRepo");

            Assert.AreEqual(_MockClock.Object.DefaultDate, result);
        }

        /// <summary>
        /// Checks whether the GetLastRunDate method returns the stored run date.
        /// </summary>
        [TestMethod]
        public async Task TestGetLastRunDate()
        {
            DateTime expectedDate = new(2026, 06, 15, 10, 30, 0, DateTimeKind.Utc);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    "IF NOT EXISTS (SELECT 1 FROM Repository WHERE [Name] = 'RunDateTest') INSERT INTO Repository ([Name]) VALUES ('RunDateTest')", connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new(
                    "INSERT INTO RunHistory (RepositoryId, RunDate, Issues, Commits, PullRequests, WorkflowRuns, Releases) " +
                    "VALUES ((SELECT RepositoryId FROM Repository WHERE [Name] = 'RunDateTest'), @runDate, 0, 0, 0, 0, 0)", connection))
                {
                    command.Parameters.AddWithValue("@runDate", expectedDate);
                    command.ExecuteNonQuery();
                }
            }

            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            DateTime result = await _service.GetLastRunDate("RunDateTest");

            Assert.AreEqual(expectedDate, result);
        }

        /// <summary>
        /// Checks whether the OutputIssues method inserts an issue into the database.
        /// </summary>
        [TestMethod]
        public async Task TestOutputIssues()
        {
            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            List<IssueModel> issues =
            [
                new()
                {
                    Repository = "OutputTest",
                    Id = 99887766,
                    Number = 42,
                    Title = "Test Issue",
                    Assignee = new() { Login = "Tester" },
                    Type = "Bug",
                    State = "Open",
                    Created_At = new DateTime(2026, 01, 15, 0, 0, 0, DateTimeKind.Utc),
                    Labels = []
                }
            ];

            await _service.OutputIssues("OutputTest", issues);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("SELECT COUNT(*) FROM Issue WHERE GitHubIssueId = 99887766", connection))
                {
                    int count = (int)command.ExecuteScalar()!;
                    Assert.AreEqual(1, count);
                }
            }
        }

        /// <summary>
        /// Checks whether the OutputCommits method inserts a commit into the database.
        /// </summary>
        [TestMethod]
        public async Task TestOutputCommits()
        {
            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            List<Models.CommitModel> commits =
            [
                new()
                {
                    Repository = "CommitTest",
                    Sha = "abc123def456unique",
                    Commit = new()
                    {
                        Author = new() { Name = "AuthorUser" },
                        Committer = new() { Name = "CommitterUser" },
                        Message = "Test commit message"
                    }
                }
            ];

            await _service.OutputCommits("CommitTest", commits);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("SELECT COUNT(*) FROM [Commit] WHERE GitHubCommitId = 'abc123def456unique'", connection))
                {
                    int count = (int)command.ExecuteScalar()!;
                    Assert.AreEqual(1, count);
                }
            }
        }

        /// <summary>
        /// Checks whether the LogRun method inserts a run history record into the database.
        /// </summary>
        [TestMethod]
        public async Task TestLogRun()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    "IF NOT EXISTS (SELECT 1 FROM Repository WHERE [Name] = 'LogRunTest') INSERT INTO Repository ([Name]) VALUES ('LogRunTest')", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            await _service.LogRun("LogRunTest", 5, 10, 3, 2, 1);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    "SELECT Issues FROM RunHistory WHERE RepositoryId = (SELECT RepositoryId FROM Repository WHERE [Name] = 'LogRunTest') ORDER BY RunHistoryId DESC", connection))
                {
                    object? result = command.ExecuteScalar();
                    Assert.IsNotNull(result);
                    Assert.AreEqual(5, (int)result);
                }
            }
        }

        /// <summary>
        /// Checks whether the LogIssueAggregates method inserts aggregate records into the database.
        /// </summary>
        [TestMethod]
        public async Task TestLogIssueAggregates()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    "IF NOT EXISTS (SELECT 1 FROM Repository WHERE [Name] = 'AggregateTest') INSERT INTO Repository ([Name]) VALUES ('AggregateTest')", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            DateTime testDate = new(2026, 08, 31, 0, 0, 0, DateTimeKind.Utc);

            List<IssueAggregateModel> aggregates =
            [
                new() { Date = testDate, Created = 3, Solved = 1 }
            ];

            await _service.LogIssueAggregates("AggregateTest", aggregates);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new(
                    "SELECT Created, Solved FROM IssueAggregate WHERE RepositoryId = (SELECT RepositoryId FROM Repository WHERE [Name] = 'AggregateTest') AND [Date] = @date", connection))
                {
                    command.Parameters.AddWithValue("@date", testDate);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Assert.IsTrue(reader.Read());
                        Assert.AreEqual(3, reader.GetInt32(0));
                        Assert.AreEqual(1, reader.GetInt32(1));
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the GetIssues method returns existing issues from the database.
        /// </summary>
        [TestMethod]
        public async Task TestGetIssues()
        {
            FileSystemWrapper _fileSystem = new();
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);
            _mockOptions.Setup(o => o.SQLFiles).Returns(_SqlFilesPath);

            DatabaseWrapper _database = new(_mockOptions.Object, _MockLogger.Object);
            DatabaseService _service = new(_MockLogger.Object, _MockClock.Object, _fileSystem, _mockOptions.Object, _database);

            List<IssueModel> issuesToInsert =
            [
                new()
                {
                    Repository = "GetIssuesTest",
                    Id = 11223344,
                    Number = 7,
                    Title = "Retrievable Issue",
                    Assignee = new() { Login = "Retriever" },
                    Type = "New Feature",
                    State = "Closed",
                    Created_At = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    Closed_At = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                    Labels = []
                }
            ];

            await _service.OutputIssues("GetIssuesTest", issuesToInsert);

            List<IssueModel>? result = await _service.GetIssues("GetIssuesTest");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            Assert.AreEqual(11223344, result[0].Id);
        }
    }
}
