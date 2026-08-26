using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Search.Search;

public sealed record SearchResponse(
    IReadOnlyList<SearchAlbumResponse> BestMatches,
    IReadOnlyList<SearchAlbumResponse> Suggestions,
    IReadOnlyList<string> SuggestedTerms,
    int TotalCount
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "bestMatches": [
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
                ],
                "suggestions": [
                    {
                        "sqid": "7bWn4R",
                        "title": "Harbour Lights",
                        "titleSlug": "harbour-lights",
                        "imageUrl": "https://cdn.halcyonrecords.example/albums/harbour-lights.jpg",
                        "releaseDate": "1997-09-02",
                        "priceInPence": 1599,
                        "originalPriceInPence": null,
                        "isNew": true,
                        "isOnSale": false,
                        "isStaffPick": false,
                        "unitsInStock": 34,
                        "isInStock": true,
                        "artists": [
                            { "sqid": "4mTb7K", "name": "The Coast Runners", "nameSlug": "the-coast-runners" }
                        ],
                        "genres": [
                            { "name": "Dream Pop", "slug": "dream-pop" }
                        ]
                    }
                ],
                "suggestedTerms": ["midnight", "dream pop", "shoegaze"],
                "totalCount": 1
            }
            """
        )!;
}

public sealed record SearchAlbumResponse(
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
    IReadOnlyList<SearchAlbumArtistResponse> Artists,
    IReadOnlyList<SearchAlbumGenreResponse> Genres
);

public sealed record SearchAlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record SearchAlbumGenreResponse(string Name, string Slug);
