using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed class CreateOrderHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    IOptions<ShopOptions> shopOptions,
    TimeProvider timeProvider,
    HybridCache cache
) : IRequestHandler<CreateOrderCommand, ErrorOr<CreateOrderResponse>>
{
    public async Task<ErrorOr<CreateOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.PublicId == command.PublicId,
            cancellationToken
        );
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        var existingOrder = await OrdersWithItemsAndAlbums()
            .FirstOrDefaultAsync(
                o => o.IdempotencyKey == command.IdempotencyKey,
                cancellationToken
            );
        if (existingOrder is not null)
        {
            return ToResponse(existingOrder);
        }

        var cart = await dbContext
            .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Album)
            .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

        if (cart is null || cart.CartItems.Count == 0)
        {
            return DomainErrors.Order.CartEmpty();
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        var orderItems = new List<OrderItem>();
        var totalInPence = 0;

        foreach (var cartItem in cart.CartItems)
        {
            var rowsAffected = await dbContext
                .Albums.Where(a => a.Id == cartItem.AlbumId && a.UnitsInStock >= cartItem.Quantity)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            a => a.UnitsInStock,
                            a => a.UnitsInStock - cartItem.Quantity
                        ),
                    cancellationToken
                );

            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                return DomainErrors.Order.InsufficientStock(cartItem.Album.Title);
            }

            orderItems.Add(
                new OrderItem
                {
                    AlbumId = cartItem.AlbumId,
                    Album = cartItem.Album,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchaseInPence = cartItem.Album.PriceInPence,
                }
            );
            totalInPence += cartItem.Album.PriceInPence * cartItem.Quantity;
        }

        var sequenceValue = (
            await dbContext
                .Database.SqlQueryRaw<int>("SELECT NEXT VALUE FOR OrderNumberSequence")
                .ToListAsync(cancellationToken)
        )[0];

        var order = new Order
        {
            UserId = user.Id,
            OrderNumber = FormatOrderNumber(sequenceValue),
            IdempotencyKey = command.IdempotencyKey,
            TotalInPence = totalInPence,
            PlacedAt = timeProvider.GetUtcNow(),
            OrderItems = orderItems,
        };
        dbContext.Orders.Add(order);
        await dbContext
            .CartItems.Where(ci => ci.CartId == cart.Id)
            .ExecuteDeleteAsync(cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsIdempotencyKeyConflict(ex))
        {
            await transaction.RollbackAsync(CancellationToken.None);
            var winningOrder = await OrdersWithItemsAndAlbums()
                .FirstAsync(o => o.IdempotencyKey == command.IdempotencyKey, cancellationToken);
            return ToResponse(winningOrder);
        }

        await transaction.CommitAsync(CancellationToken.None);

        var tags = orderItems
            .Select(oi => $"album:{albumSqids.Encode(oi.AlbumId.Value)}")
            .Append("albums")
            .ToList();
        await cache.RemoveByTagAsync(tags, CancellationToken.None);

        return ToResponse(order);
    }

    private string FormatOrderNumber(int sequenceValue) =>
        $"{shopOptions.Value.OrderNumberPrefix}-{sequenceValue.ToString().PadLeft(shopOptions.Value.OrderNumberPadding, '0')}";

    private static bool IsIdempotencyKeyConflict(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };

    private IQueryable<Order> OrdersWithItemsAndAlbums() =>
        dbContext.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Album).AsNoTracking();

    private CreateOrderResponse ToResponse(Order order) =>
        new(
            order.OrderNumber,
            order.PlacedAt,
            order.TotalInPence,
            order
                .OrderItems.Select(oi => new CreateOrderItemResponse(
                    albumSqids.Encode(oi.AlbumId.Value),
                    oi.Album.Title,
                    Slugifier.Slugify(oi.Album.Title),
                    oi.Album.ImageUrl,
                    oi.Quantity,
                    oi.PriceAtPurchaseInPence
                ))
                .ToList()
        );
}
