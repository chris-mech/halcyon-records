using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.Seed;

public static class ShowcaseAccountSeeder
{
    public static async Task SeedOrdersAsync(
        ApplicationDbContext dbContext,
        User showcaseUser,
        IOptions<ShopOptions> shopOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default
    )
    {
        var albums = await dbContext
            .Albums.OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToListAsync(cancellationToken);

        if (albums.Count < 3)
        {
            return;
        }

        var prefix = shopOptions.Value.OrderNumberPrefix;

        dbContext.Orders.AddRange(
            BuildOrder(
                showcaseUser,
                $"{prefix}-DEMO-01",
                timeProvider.GetUtcNow().AddDays(-3),
                [(albums[0], 1)]
            ),
            BuildOrder(
                showcaseUser,
                $"{prefix}-DEMO-02",
                timeProvider.GetUtcNow().AddDays(-1),
                [(albums[1], 1), (albums[2], 1)]
            )
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Order BuildOrder(
        User showcaseUser,
        string orderNumber,
        DateTimeOffset placedAt,
        (Album Album, int Quantity)[] items
    )
    {
        var orderItems = items
            .Select(item => new OrderItem
            {
                AlbumId = item.Album.Id,
                Album = item.Album,
                Quantity = item.Quantity,
                PriceAtPurchaseInPence = item.Album.PriceInPence,
            })
            .ToList();

        return new Order
        {
            UserId = showcaseUser.Id,
            OrderNumber = orderNumber,
            IdempotencyKey = Guid.NewGuid(),
            ContactFirstName = showcaseUser.FirstName,
            ContactLastName = showcaseUser.LastName,
            ContactEmail = showcaseUser.Email!,
            TotalInPence = orderItems.Sum(oi => oi.PriceAtPurchaseInPence * oi.Quantity),
            PlacedAt = placedAt,
            OrderItems = orderItems,
        };
    }
}
