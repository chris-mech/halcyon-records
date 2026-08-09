using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed class GetRelatedAlbumsHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids
) : IRequestHandler<GetRelatedAlbumsQuery, ErrorOr<IReadOnlyList<RelatedAlbumResponse>>>
{
    private const int MaxResults = 4;

    public async Task<ErrorOr<IReadOnlyList<RelatedAlbumResponse>>> Handle(
        GetRelatedAlbumsQuery query,
        CancellationToken cancellationToken
    )
    {
        if (albumSqids.Decode(query.Sqid) is not { } id)
        {
            return Error.NotFound(description: "Album not found.");
        }

        var currentId = new AlbumId(id);

        var current = await dbContext
            .Albums.Where(a => a.Id == currentId)
            .Select(a => new
            {
                a.ReleaseDate,
                GenreIds = a.AlbumGenres.Select(ag => ag.GenreId),
                ArtistIds = a.AlbumArtists.Select(aa => aa.ArtistId),
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            return Error.NotFound(description: "Album not found.");
        }

        var selectedIds = new List<AlbumId>();

        async Task FillRemainingAsync(IQueryable<Album> candidates)
        {
            if (selectedIds.Count >= MaxResults)
            {
                return;
            }

            var ids = await candidates
                .Where(a => a.Id != currentId && !selectedIds.Contains(a.Id))
                .Select(a => a.Id)
                .Take(MaxResults - selectedIds.Count)
                .ToListAsync(cancellationToken);

            selectedIds.AddRange(ids);
        }

        await FillRemainingAsync(
            dbContext
                .Albums.Where(a => a.AlbumGenres.Any(ag => current.GenreIds.Contains(ag.GenreId)))
                .OrderBy(_ => Guid.NewGuid())
        );

        await FillRemainingAsync(
            dbContext
                .Albums.Where(a =>
                    a.AlbumArtists.Any(aa => current.ArtistIds.Contains(aa.ArtistId))
                )
                .OrderBy(_ => Guid.NewGuid())
        );

        if (current.ReleaseDate is { } releaseDate)
        {
            await FillRemainingAsync(
                dbContext
                    .Albums.Where(a => a.ReleaseDate != null)
                    .OrderBy(a =>
                        Math.Abs(EF.Functions.DateDiffDay(a.ReleaseDate!.Value, releaseDate))
                    )
            );
        }

        await FillRemainingAsync(dbContext.Albums.OrderBy(_ => Guid.NewGuid()));

        var albums = await dbContext
            .Albums.Where(a => selectedIds.Contains(a.Id))
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

        var albumsById = albums.ToDictionary(a => a.Id);

        return selectedIds
            .Select(albumId => albumsById[albumId])
            .Select(a => new RelatedAlbumResponse(
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
                a.Artists.Select(artist => new RelatedAlbumArtistResponse(
                        artistSqids.Encode(artist.Id.Value),
                        artist.Name,
                        Slugifier.Slugify(artist.Name)
                    ))
                    .ToList(),
                a.Genres.Select(genre => new RelatedAlbumGenreResponse(genre.Name, genre.Slug))
                    .ToList()
            ))
            .ToList();
    }
}
