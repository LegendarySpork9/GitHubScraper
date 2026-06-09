// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using GitHubScraper.Implementations;
using GitHubScraper.Services;

namespace GitHubScraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            ILoggerService logger = new LoggerServiceWrapper();
            ApplicationService _applicationService = new(logger);

            logger.LogMessage(StandardValues.LoggerValues.Info, "Logging Started");
            logger.LogMessage(StandardValues.LoggerValues.Info, "Configuring Application");

            if (!_applicationService.Setup())
            {
                logger.LogMessage(StandardValues.LoggerValues.Info, "Logging Stopped");
                Environment.Exit(0);
            }

            logger.LogMessage(StandardValues.LoggerValues.Info, "Configured Application");
            logger.LogMessage(StandardValues.LoggerValues.Info, "Running Application");

            await _applicationService.Run();

            logger.LogMessage(StandardValues.LoggerValues.Info, "Logging Stopped");
        }
    }
}
