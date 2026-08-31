// Copyright © - 31/08/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Implementations;
using Microsoft.Data.SqlClient;
using Moq;

namespace GitHubScraper.PersistenceTests.Implementations
{
    [TestClass]
    [DoNotParallelize]
    public class DatabaseWrapperTest
    {
        private static string _DatabaseName = null!;
        private static string _ConnectionString = null!;
        private readonly Mock<ILoggerService> _MockLogger = new();

        /// <summary>
        /// Creates a LocalDB test database and a simple test table.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _DatabaseName = $"GitHubScraper_Test_{Guid.NewGuid():N}";
            string masterConnection = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;";
            _ConnectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={_DatabaseName};Integrated Security=true;";

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

                using (SqlCommand command = new(
                    "CREATE TABLE TestData (Id INT IDENTITY(1,1) PRIMARY KEY, Name VARCHAR(50) NOT NULL, Value INT NOT NULL)", connection))
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
        /// Clears the test table before each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("DELETE FROM TestData", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Checks whether the Query method returns a list of mapped results.
        /// </summary>
        [TestMethod]
        public async Task TestQuery()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("INSERT INTO TestData (Name, Value) VALUES ('Alpha', 1), ('Beta', 2)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (List<string> results, Exception? ex) = await _wrapper.Query("SELECT Name FROM TestData ORDER BY Id", reader => reader.GetString(0));

            Assert.IsNull(ex);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("Alpha", results[0]);
            Assert.AreEqual("Beta", results[1]);
        }

        /// <summary>
        /// Checks whether the QuerySingle method returns a single mapped result.
        /// </summary>
        [TestMethod]
        public async Task TestQuerySingle()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("INSERT INTO TestData (Name, Value) VALUES ('Single', 42)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (int result, Exception? ex) = await _wrapper.QuerySingle("SELECT Value FROM TestData WHERE Name = @name",
                reader => reader.GetInt32(0),
                new SqlParameter("@name", "Single"));

            Assert.IsNull(ex);
            Assert.AreEqual(42, result);
        }

        /// <summary>
        /// Checks whether the QuerySingle method returns default when no rows match.
        /// </summary>
        [TestMethod]
        public async Task TestQuerySingleNoMatch()
        {
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (int result, Exception? ex) = await _wrapper.QuerySingle("SELECT Value FROM TestData WHERE Name = @name",
                reader => reader.GetInt32(0),
                new SqlParameter("@name", "NonExistent"));

            Assert.IsNull(ex);
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Checks whether the Execute method inserts a row and returns the affected count.
        /// </summary>
        [TestMethod]
        public async Task TestExecute()
        {
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (int result, Exception? ex) = await _wrapper.Execute(
                "INSERT INTO TestData (Name, Value) VALUES (@name, @value)",
                new SqlParameter("@name", "Executed"),
                new SqlParameter("@value", 99));

            Assert.IsNull(ex);
            Assert.AreEqual(1, result);

            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("SELECT Value FROM TestData WHERE Name = 'Executed'", connection))
                {
                    object? value = command.ExecuteScalar();
                    Assert.AreEqual(99, (int)value!);
                }
            }
        }

        /// <summary>
        /// Checks whether the Query method returns an exception for invalid SQL.
        /// </summary>
        [TestMethod]
        public async Task TestQueryInvalidSql()
        {
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (List<string> results, Exception? ex) = await _wrapper.Query("SELECT FROM INVALID_TABLE", reader => reader.GetString(0));

            Assert.IsNotNull(ex);
            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Checks whether the Execute method returns an exception for an invalid connection.
        /// </summary>
        [TestMethod]
        public async Task TestExecuteInvalidConnection()
        {
            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns("Server=(localdb)\\MSSQLLocalDB;Database=NonExistentDb_12345;Integrated Security=true;");

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (int result, Exception? ex) = await _wrapper.Execute("INSERT INTO TestData (Name, Value) VALUES ('Test', 1)");

            Assert.IsNotNull(ex);
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Checks whether the Query method handles parameterised queries correctly.
        /// </summary>
        [TestMethod]
        public async Task TestQueryWithParameters()
        {
            using (SqlConnection connection = new(_ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new("INSERT INTO TestData (Name, Value) VALUES ('FilterMe', 10), ('KeepMe', 20), ('FilterMe', 30)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Mock<IDatabaseOptions> _mockOptions = new();
            _mockOptions.Setup(o => o.ConnectionString).Returns(_ConnectionString);

            DatabaseWrapper _wrapper = new(_mockOptions.Object, _MockLogger.Object);

            (List<int> results, Exception? ex) = await _wrapper.Query("SELECT Value FROM TestData WHERE Name = @name ORDER BY Value",
                reader => reader.GetInt32(0),
                new SqlParameter("@name", "FilterMe"));

            Assert.IsNull(ex);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(10, results[0]);
            Assert.AreEqual(30, results[1]);
        }
    }
}
