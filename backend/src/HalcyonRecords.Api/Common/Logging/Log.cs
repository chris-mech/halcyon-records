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
}
