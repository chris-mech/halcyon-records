using Coravel.Invocable;
using HalcyonRecords.Api.Common.Logging;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class AccountMaintenanceJob(
    DemoAccountCleaner cleaner,
    ShowcaseAccountResetter resetter,
    ILogger<AccountMaintenanceJob> logger
) : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            await cleaner.RemoveStaleAccountsAsync(CancellationToken.None);
            await resetter.ResetShowcaseAccountAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.AccountMaintenanceSweepFailed(ex);
        }
    }
}
