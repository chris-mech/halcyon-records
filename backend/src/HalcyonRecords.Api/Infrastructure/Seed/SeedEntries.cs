namespace HalcyonRecords.Api.Infrastructure.Seed;

public enum SeedSource
{
    MusicBrainz,
    Generated,
}

public sealed record ArtistSeedEntry(
    string Name,
    Guid SourceId,
    SeedSource Source,
    string? Bio,
    string? Origin,
    int? ActiveSince,
    string? ImageUrl
);

public sealed record GenreSeedEntry(string Name, string Slug, string? Description = null);

public sealed record AlbumSeedEntry(
    string Title,
    Guid SourceId,
    SeedSource Source,
    string? Description,
    DateOnly? ReleaseDate,
    string? Label,
    bool IsNew,
    bool IsStaffPick,
    string? ImageUrl,
    IReadOnlyList<Guid> ArtistSourceIds,
    IReadOnlyList<string> GenreSlugs,
    int UnitsInStock,
    int PriceInPence,
    int? OriginalPriceInPence
);
