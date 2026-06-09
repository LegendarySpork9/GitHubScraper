// Copyright © - 16/03/2026 - Toby Hunter
using GitHubScraper.Abstractions;
using GitHubScraper.Converters;
using Microsoft.Data.SqlClient;

namespace GitHubScraper.Implementations
{
    public class DatabaseWrapper : IDatabase
    {
        private readonly IDatabaseOptions _Options;
        private readonly ILoggerService _Logger;

        // Sets the class's global variables.
        public DatabaseWrapper(
            IDatabaseOptions _options,
            ILoggerService _logger)
        {
            _Options = _options;
            _Logger = _logger;
        }

        /// <summary>
        /// Returns a list of the given model from the database.
        /// </summary>
        public async Task<(List<T>, Exception?)> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            List<T> results = [];
            Exception? exception = null;

            try
            {
                using (SqlConnection connection = new(_Options.ConnectionString))
                {
                    connection.Open();

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "SQL Connection Opened");

                    using (SqlCommand command = new(sql, connection))
                    {
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Command Loaded");

                        command.Parameters.AddRange(parameters);

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Parameters Set");
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Executing Query");

                        using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                        {
                            while (dataReader.Read())
                            {
                                results.Add(map(dataReader));
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                exception = ex;
            }

            return (results, exception);
        }

        /// <summary>
        /// Returns the given field from the database.
        /// </summary>
        public async Task<(T?, Exception?)> QuerySingle<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)    
        {
            T? result = default;
            Exception? exception = null;

            try
            {
                using (SqlConnection connection = new(_Options.ConnectionString))
                {
                    connection.Open();

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "SQL Connection Opened");

                    using (SqlCommand command = new(sql, connection))
                    {
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Command Loaded");

                        command.Parameters.AddRange(parameters);

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Parameters Set");
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Executing Query");

                        using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                        {
                            if (dataReader.Read())
                            {
                                result = map(dataReader);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                exception = ex;
            }

            return (result, exception);
        }

        /// <summary>
        /// Returns the result of the execution for given query.
        /// </summary>
        public async Task<(int, Exception?)> Execute(string sql, params SqlParameter[] parameters)
        {
            int result = -1;
            Exception? exception = null;

            try
            {
                using (SqlConnection connection = new(_Options.ConnectionString))
                {
                    connection.Open();

                    _Logger.LogMessage(StandardValues.LoggerValues.Debug, "SQL Connection Opened");

                    using (SqlCommand command = new(sql, connection))
                    {
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Command Loaded");

                        command.Parameters.AddRange(parameters);

                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Parameters Set");
                        _Logger.LogMessage(StandardValues.LoggerValues.Debug, "Executing Query");

                        result = await command.ExecuteNonQueryAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                exception = ex;
            }

            return (result, exception);
        }
    }
}
