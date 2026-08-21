using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed class GetOrderByNumberHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids
) : IRequestHandler<GetOrderByNumberQuery, ErrorOr<OrderDetailResponse>>
{
    public async Task<ErrorOr<OrderDetailResponse>> Handle(
        GetOrderByNumberQuery query,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.PublicId == query.PublicId,
            cancellationToken
        );
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        var order = await dbContext
            .Orders.AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Album)
                    .ThenInclude(a => a.AlbumArtists)
                        .ThenInclude(aa => aa.Artist)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.OrderNumber == query.OrderNumber, cancellationToken);

        if (order is null || order.UserId != user.Id)
        {
            return DomainErrors.Order.NotFound($"Order '{query.OrderNumber}' not found.");
        }

        return new OrderDetailResponse(
            order.OrderNumber,
            order.PlacedAt,
            order.Status.ToString(),
            order.ContactFirstName,
            order.ContactLastName,
            order.ContactEmail,
            order.TotalInPence,
            order
                .OrderItems.Select(oi => new OrderDetailItemResponse(
                    albumSqids.Encode(oi.AlbumId.Value),
                    oi.Album.Title,
                    Slugifier.Slugify(oi.Album.Title),
                    oi.Album.AlbumArtists.OrderBy(aa => aa.Artist.Name)
                        .Select(aa => new OrderDetailItemArtistResponse(
                            artistSqids.Encode(aa.Artist.Id.Value),
                            aa.Artist.Name,
                            Slugifier.Slugify(aa.Artist.Name)
                        ))
                        .ToList(),
                    oi.Album.ImageUrl,
                    oi.Quantity,
                    oi.PriceAtPurchaseInPence
                ))
                .ToList()
        );
    }
}
