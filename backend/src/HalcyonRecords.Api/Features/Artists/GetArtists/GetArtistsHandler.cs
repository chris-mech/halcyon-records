using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Artists.GetArtists;

public sealed class GetArtistsHandler(ApplicationDbContext dbContext, ArtistSqidEncoder artistSqids)
    : IRequestHandler<GetArtistsQuery, ErrorOr<IReadOnlyList<ArtistListItemResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<ArtistListItemResponse>>> Handle(
        GetArtistsQuery query,
        CancellationToken cancellationToken
    )
    {
        var artists = await dbContext
            .Artists.OrderBy(a => a.Name)
            .Select(a => new
            {
                a.Id,
                a.Name,
                AlbumCount = a.AlbumArtists.Count,
            })
            .ToListAsync(cancellationToken);

        return artists
            .Select(a => new ArtistListItemResponse(
                artistSqids.Encode(a.Id.Value),
                a.Name,
                Slugifier.Slugify(a.Name),
                a.AlbumCount
            ))
            .ToList();
    }
}
