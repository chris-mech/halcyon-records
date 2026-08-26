using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed record AlbumSummaryResponse(
    string Sqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    DateOnly? ReleaseDate,
    int PriceInPence,
    int? OriginalPriceInPence,
    bool IsNew,
    bool IsOnSale,
    bool IsStaffPick,
    int UnitsInStock,
    bool IsInStock,
    IReadOnlyList<AlbumArtistResponse> Artists,
    IReadOnlyList<AlbumGenreResponse> Genres
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "sqid": "9pXqL2",
                "title": "Midnight Static",
                "titleSlug": "midnight-static",
                "imageUrl": "https://cdn.halcyonrecords.example/albums/midnight-static.jpg",
                "releaseDate": "1994-03-14",
                "priceInPence": 1899,
                "originalPriceInPence": 2299,
                "isNew": false,
                "isOnSale": true,
                "isStaffPick": true,
                "unitsInStock": 12,
                "isInStock": true,
                "artists": [
                    { "sqid": "4mTb7K", "name": "The Coast Runners", "nameSlug": "the-coast-runners" }
                ],
                "genres": [
                    { "name": "Dream Pop", "slug": "dream-pop" },
                    { "name": "Shoegaze", "slug": "shoegaze" }
                ]
            }
            """
        )!;
}

public sealed record AlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record AlbumGenreResponse(string Name, string Slug);
