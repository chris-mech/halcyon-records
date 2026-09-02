using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed class GetAlbumByIdHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids
) : IRequestHandler<GetAlbumByIdQuery, ErrorOr<AlbumDetailResponse>>
{
    public async Task<ErrorOr<AlbumDetailResponse>> Handle(
        GetAlbumByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        if (albumSqids.Decode(query.Sqid) is not { } id)
        {
            return DomainErrors.Album.NotFound($"Album '{query.Sqid}' not found.");
        }

        var album = await dbContext
            .Albums.AsNoTracking()
            .Include(a => a.AlbumArtists)
                .ThenInclude(aa => aa.Artist)
            .Include(a => a.AlbumGenres)
                .ThenInclude(ag => ag.Genre)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == new AlbumId(id), cancellationToken);

        if (album is null)
        {
            return DomainErrors.Album.NotFound($"Album '{query.Sqid}' not found.");
        }

        return new AlbumDetailResponse(
            albumSqids.Encode(album.Id.Value),
            album.Title,
            Slugifier.Slugify(album.Title),
            album.Description,
            album.Label,
            album.ImageUrl,
            album.ReleaseDate,
            album.PriceInPence,
            album.OriginalPriceInPence,
            album.IsNew,
            album.OriginalPriceInPence is not null,
            album.IsStaffPick,
            album.UnitsInStock,
            album.UnitsInStock > 0,
            album
                .AlbumArtists.OrderBy(aa => aa.Artist.Name)
                .Select(aa => new AlbumDetailArtistResponse(
                    artistSqids.Encode(aa.Artist.Id.Value),
                    aa.Artist.Name,
                    Slugifier.Slugify(aa.Artist.Name)
                ))
                .ToList(),
            album
                .AlbumGenres.OrderBy(ag => ag.Genre.Name)
                .Select(ag => new AlbumDetailGenreResponse(ag.Genre.Name, ag.Genre.Slug))
                .ToList()
        );
    }
}
