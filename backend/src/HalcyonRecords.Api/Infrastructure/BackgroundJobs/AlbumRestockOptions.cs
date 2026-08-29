namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class AlbumRestockOptions
{
    public const string SectionName = "AlbumRestock";

    public string CronSchedule { get; init; } = "0 */12 * * *";
}
