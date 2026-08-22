using HalcyonRecords.Api.Common.Logging;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public sealed class ShowcaseAccountResetter(
    ApplicationDbContext dbContext,
    IOptions<ShopOptions> shopOptions,
    TimeProvider timeProvider,
    ILogger<ShowcaseAccountResetter> logger
)
{
    public async Task ResetShowcaseAccountAsync(CancellationToken cancellationToken)
    {
        var showcaseUsers = await dbContext
            .Users.Where(u => u.IsShowcaseAccount)
            .ToListAsync(cancellationToken);

        var orderCutoff = timeProvider.GetUtcNow().AddHours(-1);

        foreach (var showcaseUser in showcaseUsers)
        {
            await dbContext
                .Orders.Where(o => o.UserId == showcaseUser.Id && o.PlacedAt < orderCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            var cart = await dbContext.Carts.FirstOrDefaultAsync(
                c => c.UserId == showcaseUser.Id,
                cancellationToken
            );
            if (cart is not null)
            {
                await dbContext
                    .CartItems.Where(ci => ci.CartId == cart.Id)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await ShowcaseAccountSeeder.SeedOrdersAsync(
                dbContext,
                showcaseUser,
                shopOptions,
                timeProvider,
                cancellationToken
            );

            logger.ShowcaseAccountReset(showcaseUser.Id);
        }
    }
}
