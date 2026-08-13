using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Artists.GetArtistById;

public sealed class GetArtistByIdHandler(
    ApplicationDbContext dbContext,
    ArtistSqidEncoder artistSqids,
    AlbumSqidEncoder albumSqids
) : IRequestHandler<GetArtistByIdQuery, ErrorOr<ArtistDetailResponse>>
{
    public async Task<ErrorOr<ArtistDetailResponse>> Handle(
        GetArtistByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        if (artistSqids.Decode(query.Sqid) is not { } id)
        {
            return Error.NotFound(description: "Artist not found.");
        }

        var artistId = new ArtistId(id);

        var artistData = await dbContext
            .Artists.Where(a => a.Id == artistId)
            .Select(a => new
            {
                a.Name,
                a.Bio,
                a.Origin,
                a.Type,
                a.SinceYear,
                a.ImageUrl,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (artistData is null)
        {
            return Error.NotFound(description: "Artist not found.");
        }

        var sort = Enum.Parse<ArtistAlbumSortBy>(query.Sort);

        var discography = dbContext.Albums.Where(a =>
            a.AlbumArtists.Any(aa => aa.ArtistId == artistId)
        );

        discography = sort switch
        {
            ArtistAlbumSortBy.OldestFirst => discography
                .OrderBy(a => a.ReleaseDate)
                .ThenBy(a => a.Id),
            ArtistAlbumSortBy.PriceAsc => discography
                .OrderBy(a => a.PriceInPence)
                .ThenBy(a => a.Id),
            ArtistAlbumSortBy.PriceDesc => discography
                .OrderByDescending(a => a.PriceInPence)
                .ThenBy(a => a.Id),
            _ => discography.OrderByDescending(a => a.ReleaseDate).ThenBy(a => a.Id),
        };

        var albums = await discography
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ImageUrl,
                a.ReleaseDate,
                a.PriceInPence,
                a.OriginalPriceInPence,
                a.IsNew,
                a.IsStaffPick,
                IsInStock = a.UnitsInStock > 0,
                Artists = a
                    .AlbumArtists.OrderBy(aa => aa.Artist.Name)
                    .Select(aa => new { aa.Artist.Id, aa.Artist.Name }),
                Genres = a
                    .AlbumGenres.OrderBy(ag => ag.Genre.Name)
                    .Select(ag => new { ag.Genre.Name, ag.Genre.Slug }),
            })
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var albumResponses = albums
            .Select(a => new ArtistAlbumResponse(
                albumSqids.Encode(a.Id.Value),
                a.Title,
                Slugifier.Slugify(a.Title),
                a.ImageUrl,
                a.ReleaseDate,
                a.PriceInPence,
                a.OriginalPriceInPence,
                a.IsNew,
                a.OriginalPriceInPence is not null,
                a.IsStaffPick,
                a.IsInStock,
                a.Artists.Select(artist => new ArtistAlbumArtistResponse(
                        artistSqids.Encode(artist.Id.Value),
                        artist.Name,
                        Slugifier.Slugify(artist.Name)
                    ))
                    .ToList(),
                a.Genres.Select(genre => new ArtistGenreResponse(genre.Name, genre.Slug)).ToList()
            ))
            .ToList();

        var genres = albumResponses
            .SelectMany(a => a.Genres)
            .DistinctBy(g => g.Slug)
            .OrderBy(g => g.Name)
            .ToList();

        return new ArtistDetailResponse(
            artistSqids.Encode(id),
            artistData.Name,
            Slugifier.Slugify(artistData.Name),
            artistData.Bio,
            artistData.Origin,
            artistData.Type,
            artistData.SinceYear,
            artistData.ImageUrl,
            albumResponses.Count,
            genres,
            albumResponses
        );
    }
}
