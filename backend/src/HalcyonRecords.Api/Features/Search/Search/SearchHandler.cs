using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Search;
using HalcyonRecords.Shared;
using MediatR;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MeiliSearchQuery = Meilisearch.SearchQuery;

namespace HalcyonRecords.Api.Features.Search.Search;

public sealed class SearchHandler(
    MeilisearchClient meilisearchClient,
    IOptions<SearchOptions> searchOptions,
    ApplicationDbContext dbContext,
    AlbumSqidEncoder albumSqids,
    ArtistSqidEncoder artistSqids
) : IRequestHandler<SearchQuery, ErrorOr<SearchResponse>>
{
    public async Task<ErrorOr<SearchResponse>> Handle(
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        var index = meilisearchClient.Index(searchOptions.Value.IndexName);

        var bestMatchResult = await index.SearchAsync<AlbumSearchDocument>(
            query.Q,
            new MeiliSearchQuery
            {
                Limit = searchOptions.Value.BestMatchLimit,
                RankingScoreThreshold = searchOptions.Value.BestMatchRankingScoreThreshold,
            },
            cancellationToken
        );

        var bestMatchDocuments = bestMatchResult.Hits.ToList();

        if (bestMatchDocuments.Count == 0)
        {
            return new SearchResponse(BestMatches: [], Suggestions: [], TotalCount: 0);
        }

        var bestMatchIds = bestMatchDocuments.Select(d => d.Id).ToList();
        var bestMatchGenres = bestMatchDocuments.SelectMany(d => d.Genres).Distinct().ToList();

        var suggestionIds = Array.Empty<int>();

        if (bestMatchGenres.Count > 0)
        {
            var genreFilter = string.Join(
                " OR ",
                bestMatchGenres.Select(genre => $"genres = '{EscapeFilterValue(genre)}'")
            );
            var exclusionFilter = string.Join(" AND ", bestMatchIds.Select(id => $"id != {id}"));

            var suggestionsResult = await index.SearchAsync<AlbumSearchDocument>(
                string.Empty,
                new MeiliSearchQuery
                {
                    Filter = $"({genreFilter}) AND {exclusionFilter}",
                    Limit = searchOptions.Value.SuggestionLimit,
                },
                cancellationToken
            );

            suggestionIds = suggestionsResult.Hits.Select(hit => hit.Id).ToArray();
        }

        var albumIds = bestMatchIds.Concat(suggestionIds).Select(id => new AlbumId(id)).ToArray();

        var rows = await dbContext
            .Albums.Where(a => albumIds.Contains(a.Id))
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

        var albumsById = rows.ToDictionary(a => a.Id.Value);

        SearchAlbumResponse? ToResponse(int albumId)
        {
            if (!albumsById.TryGetValue(albumId, out var a))
            {
                return null;
            }

            return new SearchAlbumResponse(
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
                a.Artists.Select(artist => new SearchAlbumArtistResponse(
                        artistSqids.Encode(artist.Id.Value),
                        artist.Name,
                        Slugifier.Slugify(artist.Name)
                    ))
                    .ToList(),
                a.Genres.Select(genre => new SearchAlbumGenreResponse(genre.Name, genre.Slug))
                    .ToList()
            );
        }

        var bestMatches = bestMatchIds.Select(ToResponse).OfType<SearchAlbumResponse>().ToList();
        var suggestions = suggestionIds.Select(ToResponse).OfType<SearchAlbumResponse>().ToList();

        return new SearchResponse(bestMatches, suggestions, bestMatches.Count + suggestions.Count);
    }

    private static string EscapeFilterValue(string value) => value.Replace("'", "\\'");
}
