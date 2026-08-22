using Coravel.Invocable;
using HalcyonRecords.Api.Common.Logging;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class AlbumRestockJob(AlbumRestocker restocker, ILogger<AlbumRestockJob> logger)
    : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            await restocker.RestockAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.AlbumRestockSweepFailed(ex);
        }
    }
}
