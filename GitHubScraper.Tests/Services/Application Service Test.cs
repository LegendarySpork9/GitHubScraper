// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Services;
using Moq;
using System.Configuration;

namespace GitHubScraper.Tests.Services
{
    [DoNotParallelize]
    [TestClass]
    public class ApplicationServiceTest
    {
        /// <summary>
        /// Sets the configuration manager up for the tests.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ConfigurationManager.AppSettings["Owner"] = "";
            ConfigurationManager.AppSettings["Repositories"] = "";
            ConfigurationManager.AppSettings["Workflows"] = "";
            ConfigurationManager.AppSettings["BearerToken"] = "";
            ConfigurationManager.AppSettings["SQLConnectionString"] = "";
            ConfigurationManager.AppSettings["SQLFiles"] = "";
        }

        /// <summary>
        /// Sets the app settings up for the tests.
        /// </summary>

        [TestInitialize]
        public void TestInitialize()
        {
            ConfigurationManager.AppSettings.Set("Owner", "");
            ConfigurationManager.AppSettings.Set("Repositories", "");
            ConfigurationManager.AppSettings.Set("Workflows", "");
            ConfigurationManager.AppSettings.Set("BearerToken", "");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "");
            ConfigurationManager.AppSettings.Set("SQLFiles", "");
        }

        /// <summary>
        /// Checks whether the configuration passes with all configuration values provided.
        /// </summary>
        [TestMethod]
        public void TestSetupPass()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsTrue(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when all the configuration values are missing.
        /// </summary>
        [TestMethod]
        public void TestSetupAllFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            Assert.IsFalse(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when the owner configuration value is missing.
        /// </summary>
        [TestMethod]
        public void TestSetupOwnerFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsFalse(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when the repository configuration value is missing.
        /// </summary>
        [TestMethod]
        public void TestSetupRepositoryFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsFalse(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration passes with all configuration values provided except the workflows.
        /// </summary>
        [TestMethod]
        public void TestSetupPassNoWorkflow()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsTrue(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when the token configuration value is missing.
        /// </summary>
        [TestMethod]
        public void TestSetupTokenFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsFalse(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when the connection string configuration value is missing.
        /// </summary>
        [TestMethod]
        public void TestSetupConnectionStringFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLFiles", @"C:\SQL");

            Assert.IsFalse(_applicationService.Setup());
        }

        /// <summary>
        /// Checks whether the configuration fails when the SQL files configuration value is missing.
        /// </summary>
        [TestMethod]
        public void TestSetupSQLFail()
        {
            Mock<ILoggerService> _mockLogger = new();

            ApplicationService _applicationService = new(_mockLogger.Object);

            ConfigurationManager.AppSettings.Set("Owner", "UnitTester");
            ConfigurationManager.AppSettings.Set("Repositories", "Unit-Test");
            ConfigurationManager.AppSettings.Set("Workflows", "Unit Test.yml");
            ConfigurationManager.AppSettings.Set("BearerToken", "This is a token");
            ConfigurationManager.AppSettings.Set("SQLConnectionString", "This is a connection string");

            Assert.IsFalse(_applicationService.Setup());
        }
    }
}
