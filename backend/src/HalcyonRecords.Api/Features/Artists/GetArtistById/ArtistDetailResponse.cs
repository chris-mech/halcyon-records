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
);

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
