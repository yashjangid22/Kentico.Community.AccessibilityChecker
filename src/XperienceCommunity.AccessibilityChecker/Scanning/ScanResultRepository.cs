using System.Data;
using System.Text.Json;

using CMS.DataEngine;

using XperienceCommunity.AccessibilityChecker.Models;

namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    public interface IScanResultRepository
    {
        Task<IReadOnlyList<ScanResultDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpsertAsync(ScanResultDto result, CancellationToken cancellationToken = default);
        Task DeleteAsync(string url, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Persists scan results using CMS.DataEngine.ConnectionHelper - plain parameterized SQL
    /// against Kentico's already-configured database connection, rather than the full Info-object
    /// system (which expects a human to provision the data class through the admin UI first, not
    /// a NuGet package silently creating one from code). Table creation is idempotent and lazy,
    /// so it doesn't depend on module init ordering.
    /// </summary>
    internal sealed class ScanResultRepository : IScanResultRepository
    {
        private const string TableName = "XperienceCommunity_AccessibilityScanResult";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly SemaphoreSlim initLock = new(1, 1);
        private bool tableEnsured;

        public async Task<IReadOnlyList<ScanResultDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await EnsureTableAsync(cancellationToken);

            var results = new List<ScanResultDto>();
            await using var reader = await ConnectionHelper.ExecuteReaderAsync(
                $"SELECT Url, Score, LastScannedOn, IssuesJson FROM {TableName} ORDER BY LastScannedOn DESC",
                new QueryDataParameters(),
                QueryTypeEnum.SQLQuery,
                CommandBehavior.Default,
                cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var issuesJson = reader.GetString(reader.GetOrdinal("IssuesJson"));
                var issues = JsonSerializer.Deserialize<IssuesBySeverityDto>(issuesJson, JsonOptions) ?? new IssuesBySeverityDto();

                results.Add(new ScanResultDto
                {
                    Url = reader.GetString(reader.GetOrdinal("Url")),
                    Score = reader.GetInt32(reader.GetOrdinal("Score")),
                    Timestamp = reader.GetDateTime(reader.GetOrdinal("LastScannedOn")),
                    IssuesBySeverity = issues
                });
            }

            return results;
        }

        public async Task UpsertAsync(ScanResultDto result, CancellationToken cancellationToken = default)
        {
            await EnsureTableAsync(cancellationToken);

            var issuesJson = JsonSerializer.Serialize(result.IssuesBySeverity, JsonOptions);

            var existsParameters = new QueryDataParameters();
            existsParameters.Add("@Url", result.Url);
            var existingId = await ConnectionHelper.ExecuteScalarAsync(
                $"SELECT ScanResultID FROM {TableName} WHERE Url = @Url",
                existsParameters,
                QueryTypeEnum.SQLQuery,
                cancellationToken);

            var writeParameters = new QueryDataParameters();
            writeParameters.Add("@Url", result.Url);
            writeParameters.Add("@Score", result.Score);
            writeParameters.Add("@LastScannedOn", result.Timestamp);
            writeParameters.Add("@IssuesJson", issuesJson);

            var query = existingId is null or DBNull
                ? $"INSERT INTO {TableName} (Url, Score, LastScannedOn, IssuesJson) VALUES (@Url, @Score, @LastScannedOn, @IssuesJson)"
                : $"UPDATE {TableName} SET Score = @Score, LastScannedOn = @LastScannedOn, IssuesJson = @IssuesJson WHERE Url = @Url";

            await ConnectionHelper.ExecuteNonQueryAsync(query, writeParameters, QueryTypeEnum.SQLQuery, cancellationToken);
        }

        public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            await EnsureTableAsync(cancellationToken);

            var parameters = new QueryDataParameters();
            parameters.Add("@Url", url);
            await ConnectionHelper.ExecuteNonQueryAsync(
                $"DELETE FROM {TableName} WHERE Url = @Url",
                parameters,
                QueryTypeEnum.SQLQuery,
                cancellationToken);
        }

        private async Task EnsureTableAsync(CancellationToken cancellationToken)
        {
            if (tableEnsured)
            {
                return;
            }

            await initLock.WaitAsync(cancellationToken);
            try
            {
                if (tableEnsured)
                {
                    return;
                }

                var ddl = $@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{TableName}')
BEGIN
    CREATE TABLE {TableName} (
        ScanResultID INT IDENTITY(1,1) PRIMARY KEY,
        Url NVARCHAR(2048) NOT NULL,
        Score INT NOT NULL,
        LastScannedOn DATETIME2 NOT NULL,
        IssuesJson NVARCHAR(MAX) NOT NULL
    )
END";
                await ConnectionHelper.ExecuteNonQueryAsync(ddl, new QueryDataParameters(), QueryTypeEnum.SQLQuery, cancellationToken);
                tableEnsured = true;
            }
            finally
            {
                initLock.Release();
            }
        }
    }
}
