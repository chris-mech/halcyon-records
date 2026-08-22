namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class AccountMaintenanceOptions
{
    public const string SectionName = "AccountMaintenance";

    public int InactivityThresholdDays { get; init; } = 7;
    public int MaxAccountAgeDays { get; init; } = 90;

    public string CronSchedule { get; init; } = "15 3 * * *";
}
