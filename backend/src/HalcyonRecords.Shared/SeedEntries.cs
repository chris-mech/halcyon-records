namespace HalcyonRecords.Shared;

public enum SeedSource
{
    MusicBrainz,
    Generated,
}

public enum ArtistType
{
    Person,
    Group,
    Orchestra,
    Choir,
    Character,
    Other,
}

public sealed record ArtistSeedEntry(
    string Name,
    ArtistMbid SourceId,
    SeedSource Source,
    string? Bio,
    string? Origin,
    ArtistType? Type,
    int? SinceYear,
    string? ImageUrl
);

public sealed record GenreSeedEntry(string Name, GenreSlug Slug, string? Description = null);

public sealed record AlbumSeedEntry(
    string Title,
    ReleaseMbid SourceId,
    SeedSource Source,
    string? Description,
    DateOnly? ReleaseDate,
    string? Label,
    bool IsNew,
    bool IsStaffPick,
    string? ImageUrl,
    IReadOnlyList<ArtistMbid> ArtistSourceIds,
    IReadOnlyList<GenreSlug> GenreSlugs,
    int UnitsInStock,
    int PriceInPence,
    int? OriginalPriceInPence
);
