// Copyright © - 31/08/2026 - Toby Hunter
using GitHubScraper.Implementations;

namespace GitHubScraper.PersistenceTests.Implementations
{
    [TestClass]
    public class SystemClockProviderTest
    {
        /// <summary>
        /// Checks whether the UtcNow property returns a UTC date and time.
        /// </summary>
        [TestMethod]
        public void TestUtcNowReturnsUtcKind()
        {
            SystemClockProvider _clock = new();

            DateTime result = _clock.UtcNow;

            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
        }

        /// <summary>
        /// Checks whether the UtcNow property returns a date close to the current time.
        /// </summary>
        [TestMethod]
        public void TestUtcNowReturnsCurrentTime()
        {
            SystemClockProvider _clock = new();

            DateTime before = DateTime.UtcNow;
            DateTime result = _clock.UtcNow;
            DateTime after = DateTime.UtcNow;

            Assert.IsTrue(result >= before);
            Assert.IsTrue(result <= after);
        }

        /// <summary>
        /// Checks whether the DefaultDate property returns the expected date.
        /// </summary>
        [TestMethod]
        public void TestDefaultDateReturnsExpectedDate()
        {
            SystemClockProvider _clock = new();

            DateTime result = _clock.DefaultDate;

            Assert.AreEqual(new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc), result);
        }
    }
}
