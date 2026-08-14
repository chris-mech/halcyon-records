using Meilisearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed class MeilisearchHealthCheck(MeilisearchClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var isHealthy = await client.IsHealthyAsync(cancellationToken);
            return isHealthy ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
