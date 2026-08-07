namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed record AlbumDetailResponse(
    string Sqid,
    string Title,
    string TitleSlug,
    string? Description,
    string? Label,
    string? ImageUrl,
    DateOnly? ReleaseDate,
    int PriceInPence,
    int? OriginalPriceInPence,
    bool IsNew,
    bool IsOnSale,
    bool IsStaffPick,
    bool IsInStock,
    IReadOnlyList<AlbumDetailArtistResponse> Artists,
    IReadOnlyList<AlbumDetailGenreResponse> Genres
);

public sealed record AlbumDetailArtistResponse(string Sqid, string Name);

public sealed record AlbumDetailGenreResponse(string Name, string Slug);
