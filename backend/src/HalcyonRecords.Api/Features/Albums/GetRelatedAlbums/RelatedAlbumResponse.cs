using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed record RelatedAlbumResponse(
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
    IReadOnlyList<RelatedAlbumArtistResponse> Artists,
    IReadOnlyList<RelatedAlbumGenreResponse> Genres
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
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
            """
        )!;
}

public sealed record RelatedAlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record RelatedAlbumGenreResponse(string Name, string Slug);
