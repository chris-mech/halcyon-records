namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public sealed record DiscogsArtist(string? Profile, IReadOnlyList<DiscogsImage>? Images);

public sealed record DiscogsImage(string? Type, string? Uri);

public sealed record DiscogsMaster(IReadOnlyList<string>? Genres, IReadOnlyList<string>? Styles);

public sealed record DiscogsSearchResponse(IReadOnlyList<DiscogsSearchResult>? Results);

public sealed record DiscogsSearchResult(
    long? Id,
    string? Title,
    IReadOnlyList<string>? Genre,
    IReadOnlyList<string>? Style,
    string? CoverImage,
    DiscogsCommunity? Community
);

public sealed record DiscogsCommunity(int? Want, int? Have);
