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
    bool IsInStock,
    IReadOnlyList<AlbumArtistResponse> Artists,
    IReadOnlyList<AlbumGenreResponse> Genres
);

public sealed record AlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record AlbumGenreResponse(string Name, string Slug);
