using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.Features.Artists.GetArtistById;

public sealed record ArtistDetailResponse(
    string Sqid,
    string Name,
    string NameSlug,
    string? Bio,
    string? Origin,
    ArtistType? Type,
    int? SinceYear,
    string? ImageUrl,
    int AlbumCount,
    IReadOnlyList<ArtistGenreResponse> Genres,
    IReadOnlyList<ArtistAlbumResponse> Albums
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "sqid": "4mTb7K",
                "name": "The Coast Runners",
                "nameSlug": "the-coast-runners",
                "bio": "A four-piece formed in Brighton in 1991, blending jangly guitars with motorik rhythms.",
                "origin": "Brighton, UK",
                "type": "Group",
                "sinceYear": 1991,
                "imageUrl": "https://cdn.halcyonrecords.example/artists/the-coast-runners.jpg",
                "albumCount": 2,
                "genres": [
                    { "name": "Dream Pop", "slug": "dream-pop" },
                    { "name": "Shoegaze", "slug": "shoegaze" }
                ],
                "albums": [
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
                    },
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
                ]
            }
            """
        )!;
}

public sealed record ArtistGenreResponse(string Name, string Slug);

public sealed record ArtistAlbumResponse(
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
    IReadOnlyList<ArtistAlbumArtistResponse> Artists,
    IReadOnlyList<ArtistGenreResponse> Genres
);

public sealed record ArtistAlbumArtistResponse(string Sqid, string Name, string NameSlug);
