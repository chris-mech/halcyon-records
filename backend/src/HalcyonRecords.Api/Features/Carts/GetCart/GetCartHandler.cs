using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Carts.GetCart;

public sealed class GetCartHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids
) : IRequestHandler<GetCartQuery, ErrorOr<IReadOnlyList<CartItemResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<CartItemResponse>>> Handle(
        GetCartQuery query,
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

        var items = await dbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .OrderBy(ci => ci.AlbumId)
            .Select(ci => new
            {
                ci.Quantity,
                ci.Album.Id,
                ci.Album.Title,
                ci.Album.ImageUrl,
                ci.Album.PriceInPence,
                ci.Album.OriginalPriceInPence,
                ci.Album.UnitsInStock,
                Artists = ci
                    .Album.AlbumArtists.OrderBy(aa => aa.Artist.Name)
                    .Select(aa => new { aa.Artist.Id, aa.Artist.Name }),
            })
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new CartItemResponse(
                albumSqids.Encode(item.Id.Value),
                item.Title,
                Slugifier.Slugify(item.Title),
                item.ImageUrl,
                item.PriceInPence,
                item.OriginalPriceInPence,
                item.Quantity,
                item.UnitsInStock,
                item.UnitsInStock > 0,
                item.Artists.Select(artist => new CartItemArtistResponse(
                        artistSqids.Encode(artist.Id.Value),
                        artist.Name,
                        Slugifier.Slugify(artist.Name)
                    ))
                    .ToList()
            ))
            .ToList();
    }
}
