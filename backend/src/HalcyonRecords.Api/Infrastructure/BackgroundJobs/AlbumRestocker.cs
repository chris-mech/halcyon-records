using HalcyonRecords.Api.Common.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class AlbumRestocker(
    ApplicationDbContext dbContext,
    HybridCache cache,
    ILogger<AlbumRestocker> logger
)
{
    public async Task<int> RestockAsync(CancellationToken cancellationToken)
    {
        var restocked = await dbContext
            .Albums.Where(a => a.UnitsInStock != a.RestockUnitsInStock)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(a => a.UnitsInStock, a => a.RestockUnitsInStock),
                cancellationToken
            );

        if (restocked > 0)
        {
            await cache.RemoveByTagAsync(["albums"], cancellationToken);
        }

        logger.AlbumsRestocked(restocked);

        return restocked;
    }
}
