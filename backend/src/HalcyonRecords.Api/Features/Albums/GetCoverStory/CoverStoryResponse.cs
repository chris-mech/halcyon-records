namespace HalcyonRecords.Api.Features.Albums.GetCoverStory;

public sealed record CoverStoryResponse(
    string Sqid,
    string Title,
    string TitleSlug,
    string? Description,
    string? ImageUrl,
    DateOnly? ReleaseDate,
    int PriceInPence,
    int? OriginalPriceInPence,
    bool IsNew,
    bool IsOnSale,
    bool IsStaffPick,
    bool IsInStock,
    int IssueNumber,
    IReadOnlyList<CoverStoryArtistResponse> Artists,
    IReadOnlyList<CoverStoryGenreResponse> Genres
);

public sealed record CoverStoryArtistResponse(string Sqid, string Name, string NameSlug);

public sealed record CoverStoryGenreResponse(string Name, string Slug);
