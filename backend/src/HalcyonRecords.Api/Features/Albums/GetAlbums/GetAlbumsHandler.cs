using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sqids;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class GetAlbumsHandler(ApplicationDbContext dbContext, SqidsEncoder<int> sqids)
    : IRequestHandler<GetAlbumsQuery, ErrorOr<PagedResult<AlbumSummaryResponse>>>
{
    public async Task<ErrorOr<PagedResult<AlbumSummaryResponse>>> Handle(
        GetAlbumsQuery query,
        CancellationToken cancellationToken
    )
    {
        var albums = dbContext.Albums.AsQueryable();

        if (query.IsNew)
        {
            albums = albums.Where(a => a.IsNew);
        }

        if (query.IsOnSale)
        {
            albums = albums.Where(a => a.OriginalPriceInPence != null);
        }

        if (query.IsStaffPick)
        {
            albums = albums.Where(a => a.IsStaffPick);
        }

        if (query.InStock)
        {
            albums = albums.Where(a => a.UnitsInStock > 0);
        }

        if (query.Genres is { Count: > 0 })
        {
            albums = albums.Where(a =>
                a.AlbumGenres.Any(ag => query.Genres.Contains(ag.Genre.Slug))
            );
        }

        var sort = Enum.Parse<AlbumSortBy>(query.Sort);

        albums = sort switch
        {
            AlbumSortBy.OldestFirst => albums.OrderBy(a => a.ReleaseDate).ThenBy(a => a.Id),
            AlbumSortBy.PriceAsc => albums.OrderBy(a => a.PriceInPence).ThenBy(a => a.Id),
            AlbumSortBy.PriceDesc => albums
                .OrderByDescending(a => a.PriceInPence)
                .ThenBy(a => a.Id),
            AlbumSortBy.ArtistAZ => albums
                .OrderBy(a =>
                    a.AlbumArtists.OrderBy(aa => aa.Artist.Name)
                        .Select(aa => aa.Artist.Name)
                        .FirstOrDefault()
                )
                .ThenBy(a => a.Id),
            AlbumSortBy.ArtistZA => albums
                .OrderByDescending(a =>
                    a.AlbumArtists.OrderBy(aa => aa.Artist.Name)
                        .Select(aa => aa.Artist.Name)
                        .FirstOrDefault()
                )
                .ThenBy(a => a.Id),
            _ => albums.OrderByDescending(a => a.ReleaseDate).ThenBy(a => a.Id),
        };
        var totalCount = await albums.CountAsync(cancellationToken);

        var page = await albums
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
            .ToListAsync(cancellationToken);

        var items = page.Select(a => new AlbumSummaryResponse(
                sqids.Encode(a.Id.Value),
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
                a.Artists.Select(artist => new AlbumArtistResponse(
                        sqids.Encode(artist.Id.Value),
                        artist.Name
                    ))
                    .ToList(),
                a.Genres.Select(genre => new AlbumGenreResponse(genre.Name, genre.Slug)).ToList()
            ))
            .ToList();

        return new PagedResult<AlbumSummaryResponse>(items, query.Page, query.PageSize, totalCount);
    }
}
