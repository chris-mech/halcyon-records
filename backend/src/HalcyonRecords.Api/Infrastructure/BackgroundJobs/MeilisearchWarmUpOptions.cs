namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class MeilisearchWarmUpOptions
{
    public const string SectionName = "MeilisearchWarmUp";

    public string CronSchedule { get; init; } = "* * * * *";
}
