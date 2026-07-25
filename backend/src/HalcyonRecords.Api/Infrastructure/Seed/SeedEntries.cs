namespace HalcyonRecords.Api.Infrastructure.Seed;

public sealed record ArtistSeedEntry(
    string Name,
    string Slug,
    string? Bio,
    string? Origin,
    int? ActiveSince,
    string? ImageUrl
);

public sealed record GenreSeedEntry(string Name, string Slug);

public sealed record AlbumSeedEntry(
    string Title,
    string Slug,
    string? Description,
    DateOnly? ReleaseDate,
    string? Label,
    bool IsNew,
    bool IsStaffPick,
    string? ImageUrl,
    IReadOnlyList<string> ArtistSlugs,
    IReadOnlyList<string> GenreSlugs,
    int UnitsInStock,
    int PriceInPence,
    int? OriginalPriceInPence
);
