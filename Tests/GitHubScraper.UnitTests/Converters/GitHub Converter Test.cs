// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Converters;

namespace GitHubScraper.UnitTests.Converters
{
    [TestClass]
    public class GitHubConverterTest
    {
        #region IsType

        /// <summary>
        /// Tests whether the IsType method returns false when given any value.
        /// </summary>
        [TestMethod]
        public void TestIsType()
        {
            Assert.IsFalse(GitHubConverter.IsType("Trombone"));
        }

        /// <summary>
        /// Tests whether the IsType method returns true when given "bug".
        /// </summary>
        [TestMethod]
        public void TestIsTypeBug()
        {
            Assert.IsTrue(GitHubConverter.IsType("bug"));
        }

        /// <summary>
        /// Tests whether the IsType method returns true when given "enhancement".
        /// </summary>
        [TestMethod]
        public void TestIsTypeEnhancement()
        {
            Assert.IsTrue(GitHubConverter.IsType("enhancement"));
        }

        /// <summary>
        /// Tests whether the IsType method returns true when given "documentation".
        /// </summary>
        [TestMethod]
        public void TestIsTypeDocumentation()
        {
            Assert.IsTrue(GitHubConverter.IsType("documentation"));
        }

        #endregion

        #region GetType

        /// <summary>
        /// Tests whether the GetType method returns the value it's given when given any value.
        /// </summary>
        [TestMethod]
        public void TestGetType()
        {
            string expected = "Trombone";
            string actual = GitHubConverter.GetType("Trombone");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetType method returns "Bug" when given "bug".
        /// </summary>
        [TestMethod]
        public void TestGetTypeBug()
        {
            string expected = "Bug";
            string actual = GitHubConverter.GetType("bug");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetType method returns "New Feature" when given "enhancement".
        /// </summary>
        [TestMethod]
        public void TestGetTypeEnhancement()
        {
            string expected = "New Feature";
            string actual = GitHubConverter.GetType("enhancement");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetType method returns "Documentation" when given "documentation".
        /// </summary>
        [TestMethod]
        public void TestGetTypeDocumentation()
        {
            string expected = "Documentation";
            string actual = GitHubConverter.GetType("documentation");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region GetQuery

        /// <summary>
        /// Tests whether the GetQuery method returns an empty string when given any value.
        /// </summary>
        [TestMethod]
        public void TestGetQuery()
        {
            string expected = string.Empty;
            string actual = GitHubConverter.GetQuery("Trombone");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/issues".
        /// </summary>
        [TestMethod]
        public void TestGetQueryIssues()
        {
            string expected = "?state=all&sort=updated&per_page=100";
            string actual = GitHubConverter.GetQuery("/issues");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/branches".
        /// </summary>
        [TestMethod]
        public void TestGetQueryBranches()
        {
            string expected = "?per_page=100";
            string actual = GitHubConverter.GetQuery("/branches");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/commits".
        /// </summary>
        [TestMethod]
        public void TestGetQueryCommits()
        {
            string expected = "?per_page=100";
            string actual = GitHubConverter.GetQuery("/commits");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/pulls".
        /// </summary>
        [TestMethod]
        public void TestGetQueryPulls()
        {
            string expected = "?state=all&sort=updated&direction=desc&per_page=100";
            string actual = GitHubConverter.GetQuery("/pulls");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/runs".
        /// </summary>
        [TestMethod]
        public void TestGetQueryRuns()
        {
            string expected = "?per_page=100";
            string actual = GitHubConverter.GetQuery("/runs");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether the GetQuery method returns the correct value when given "/releases".
        /// </summary>
        [TestMethod]
        public void TestGetQueryReleases()
        {
            string expected = "?per_page=100";
            string actual = GitHubConverter.GetQuery("/releases");

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
