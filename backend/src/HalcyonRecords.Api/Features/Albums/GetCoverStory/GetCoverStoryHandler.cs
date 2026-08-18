using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Albums.GetCoverStory;

public sealed class GetCoverStoryHandler(
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids,
    TimeProvider timeProvider
) : IRequestHandler<GetCoverStoryQuery, ErrorOr<CoverStoryResponse>>
{
    private static readonly DateOnly s_epoch = new(2026, 8, 10);

    public async Task<ErrorOr<CoverStoryResponse>> Handle(
        GetCoverStoryQuery query,
        CancellationToken cancellationToken
    )
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var weekIndex = (today.DayNumber - s_epoch.DayNumber) / 7;

        var staffPickIds = await dbContext
            .Albums.Where(a => a.IsStaffPick)
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        AlbumId selectedId;

        if (staffPickIds.Count > 0)
        {
            selectedId = staffPickIds[weekIndex % staffPickIds.Count];
        }
        else
        {
            var fallbackId = await dbContext
                .Albums.OrderByDescending(a => a.ReleaseDate)
                .ThenBy(a => a.Id)
                .Select(a => (AlbumId?)a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (fallbackId is null)
            {
                return Error.NotFound(description: "No albums available.");
            }

            selectedId = fallbackId.Value;
        }

        var album = await dbContext
            .Albums.AsNoTracking()
            .Include(a => a.AlbumArtists)
                .ThenInclude(aa => aa.Artist)
            .Include(a => a.AlbumGenres)
                .ThenInclude(ag => ag.Genre)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == selectedId, cancellationToken);

        if (album is null)
        {
            return Error.NotFound(description: "No albums available.");
        }

        return new CoverStoryResponse(
            albumSqids.Encode(album.Id.Value),
            album.Title,
            Slugifier.Slugify(album.Title),
            album.Description,
            album.ImageUrl,
            album.ReleaseDate,
            album.PriceInPence,
            album.OriginalPriceInPence,
            album.IsNew,
            album.OriginalPriceInPence is not null,
            album.IsStaffPick,
            album.UnitsInStock,
            album.UnitsInStock > 0,
            weekIndex + 1,
            album
                .AlbumArtists.OrderBy(aa => aa.Artist.Name)
                .Select(aa => new CoverStoryArtistResponse(
                    artistSqids.Encode(aa.Artist.Id.Value),
                    aa.Artist.Name,
                    Slugifier.Slugify(aa.Artist.Name)
                ))
                .ToList(),
            album
                .AlbumGenres.OrderBy(ag => ag.Genre.Name)
                .Select(ag => new CoverStoryGenreResponse(ag.Genre.Name, ag.Genre.Slug))
                .ToList()
        );
    }
}
