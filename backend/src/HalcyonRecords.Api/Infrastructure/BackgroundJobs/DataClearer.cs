using HalcyonRecords.Api.Common.Logging;
using HalcyonRecords.Api.Infrastructure.Sql;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class DataClearer(ApplicationDbContext dbContext, ILogger<DataClearer> logger)
{
    public async Task ClearAllAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.'
                + QUOTENAME(name) + N' NOCHECK CONSTRAINT ALL;' + CHAR(10)
            FROM sys.tables;
            EXEC sp_executesql @sql;
            """,
            cancellationToken
        );

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'DELETE FROM ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.'
                + QUOTENAME(name) + N';' + CHAR(10)
            FROM sys.tables
            WHERE name <> N'__EFMigrationsHistory';
            EXEC sp_executesql @sql;
            """,
            cancellationToken
        );

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.'
                + QUOTENAME(name) + N' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(10)
            FROM sys.tables;
            EXEC sp_executesql @sql;
            """,
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        logger.AllDataCleared();
    }
}
