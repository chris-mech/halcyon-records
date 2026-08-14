namespace HalcyonRecords.Api.Features.Search.Search;

public sealed record SearchResponse(
    IReadOnlyList<SearchAlbumResponse> BestMatches,
    IReadOnlyList<SearchAlbumResponse> Suggestions,
    int TotalCount
);

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
    bool IsInStock,
    IReadOnlyList<SearchAlbumArtistResponse> Artists,
    IReadOnlyList<SearchAlbumGenreResponse> Genres
);

public sealed record SearchAlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record SearchAlbumGenreResponse(string Name, string Slug);
