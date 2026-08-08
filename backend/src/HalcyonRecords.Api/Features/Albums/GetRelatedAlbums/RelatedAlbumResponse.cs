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
    bool IsInStock,
    IReadOnlyList<RelatedAlbumArtistResponse> Artists,
    IReadOnlyList<RelatedAlbumGenreResponse> Genres
);

public sealed record RelatedAlbumArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record RelatedAlbumGenreResponse(string Name, string Slug);
