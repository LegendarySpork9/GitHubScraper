// Copyright © - 31/08/2026 - Toby Hunter
using GitHubScraper.Implementations;

namespace GitHubScraper.PersistenceTests.Implementations
{
    [TestClass]
    public class FileSystemWrapperTest
    {
        private string _TempDirectory = null!;

        /// <summary>
        /// Creates a temporary directory for test isolation.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_TempDirectory);
        }

        /// <summary>
        /// Removes the temporary directory after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_TempDirectory))
            {
                Directory.Delete(_TempDirectory, true);
            }
        }

        /// <summary>
        /// Checks whether the ReadAllText method returns the contents of a file.
        /// </summary>
        [TestMethod]
        public async Task TestReadAllText()
        {
            FileSystemWrapper _wrapper = new();

            string filePath = Path.Combine(_TempDirectory, "test.sql");
            string expected = "SELECT * FROM TestTable WHERE Id = @id";
            await File.WriteAllTextAsync(filePath, expected);

            string actual = await _wrapper.ReadAllText(filePath);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Checks whether the ReadAllText method throws when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestReadAllTextFileNotFound()
        {
            FileSystemWrapper _wrapper = new();

            string filePath = Path.Combine(_TempDirectory, "nonexistent.sql");

            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            {
                await _wrapper.ReadAllText(filePath);
            });
        }

        /// <summary>
        /// Checks whether the ReadAllText method reads a multi-line SQL file correctly.
        /// </summary>
        [TestMethod]
        public async Task TestReadAllTextMultiLine()
        {
            FileSystemWrapper _wrapper = new();

            string filePath = Path.Combine(_TempDirectory, "multiline.sql");
            string expected = "SELECT Id, Name\r\nFROM TestTable\r\nWHERE Status = @status";
            await File.WriteAllTextAsync(filePath, expected);

            string actual = await _wrapper.ReadAllText(filePath);

            Assert.AreEqual(expected, actual);
        }
    }
}
