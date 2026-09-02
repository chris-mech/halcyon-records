using Coravel.Invocable;
using HalcyonRecords.Api.Common.Logging;
using Meilisearch;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class MeilisearchWarmUpJob(
    MeilisearchClient client,
    ILogger<MeilisearchWarmUpJob> logger
) : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            await client.IsHealthyAsync();
            logger.JobCompleted("MeilisearchWarmUp");
        }
        catch (Exception ex)
        {
            logger.MeilisearchWarmUpFailed(ex);
        }
    }
}
