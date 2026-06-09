// Copyright © - 16/03/2026 - Toby Hunter
using log4net;

namespace GitHubScraper.Services
{
    internal class LoggerService
    {
        private readonly ILog Logger = LogManager.GetLogger("Logs");

        /// <summary>
        /// Sends a meessage to the specified logs.
        /// </summary>
        public void LogMessage(string level, string message)
        {
            switch (level)
            {
                case "Info": Logger.Info(message.Trim()); break;
                case "Debug": Logger.Debug(message.Trim()); break;
                case "Warn": Logger.Warn(message.Trim()); break;
                case "Error": Logger.Error(message.Trim()); break;
            }
        }
    }
}
