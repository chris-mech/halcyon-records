namespace HalcyonRecords.Api.Common.Logging;

public static partial class Log
{
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred while processing the request."
    )]
    public static partial void UnhandledException(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Demo account cleanup removed {Count} account(s) inactive since before {InactivityCutoff} or registered before {MaxAgeCutoff}"
    )]
    public static partial void DemoAccountsCleaned(
        this ILogger logger,
        int count,
        DateTimeOffset inactivityCutoff,
        DateTimeOffset maxAgeCutoff
    );

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Reset showcase account {UserId} to its showcase state"
    )]
    public static partial void ShowcaseAccountReset(this ILogger logger, int userId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Account maintenance sweep failed"
    )]
    public static partial void AccountMaintenanceSweepFailed(
        this ILogger logger,
        Exception exception
    );

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Restocked {Count} album(s) to their restock level"
    )]
    public static partial void AlbumsRestocked(this ILogger logger, int count);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Error, Message = "Album restock sweep failed")]
    public static partial void AlbumRestockSweepFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Job {JobName} starting"
    )]
    public static partial void JobStarting(this ILogger logger, string jobName);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Job {JobName} completed"
    )]
    public static partial void JobCompleted(this ILogger logger, string jobName);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Error, Message = "Job {JobName} failed")]
    public static partial void JobFailed(this ILogger logger, Exception exception, string jobName);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Error, Message = "Unknown job {JobName}")]
    public static partial void JobUnknown(this ILogger logger, string jobName);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Warning,
        Message = "Meilisearch warm-up ping failed"
    )]
    public static partial void MeilisearchWarmUpFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Cleared all catalogue, order, cart, and account data ahead of a reseed"
    )]
    public static partial void AllDataCleared(this ILogger logger);
}
