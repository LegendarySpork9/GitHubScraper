// Copyright © - 16/03/2026 - Toby Hunter
using Microsoft.Data.SqlClient;

namespace GitHubScraper.Abstractions
{
    /// <summary>
    /// Interface for the database.
    /// </summary>
    public interface IDatabase
    {
        Task<(List<T>, Exception?)> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
        Task<(T?, Exception?)> QuerySingle<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
        Task<(int, Exception?)> Execute(string sql, params SqlParameter[] parameters);
    }
}