using HalcyonRecords.Api.Common.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class DemoAccountCleaner(
    ApplicationDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<AccountMaintenanceOptions> options,
    ILogger<DemoAccountCleaner> logger
)
{
    public async Task<int> RemoveStaleAccountsAsync(CancellationToken cancellationToken)
    {
        var inactivityCutoff = timeProvider
            .GetUtcNow()
            .AddDays(-options.Value.InactivityThresholdDays);
        var maxAgeCutoff = timeProvider.GetUtcNow().AddDays(-options.Value.MaxAccountAgeDays);

        var removed = await dbContext
            .Users.Where(u =>
                !u.IsShowcaseAccount
                && (u.LastActiveAt < inactivityCutoff || u.RegisteredAt < maxAgeCutoff)
            )
            .ExecuteDeleteAsync(cancellationToken);

        logger.DemoAccountsCleaned(removed, inactivityCutoff, maxAgeCutoff);

        return removed;
    }
}
