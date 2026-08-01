namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public sealed record MusicBrainzArtistFields(
    Guid SourceId,
    string Name,
    string? Origin,
    int? ActiveSince,
    long? DiscogsArtistId,
    string? WikidataQid
);

public sealed record MusicBrainzReleaseFields(
    Guid SourceId,
    string Title,
    DateOnly? ReleaseDate,
    string? Label,
    IReadOnlyList<Guid> ArtistCreditIds,
    Guid? ReleaseGroupId
);

public sealed record MusicBrainzReleaseGroupFields(long? DiscogsMasterId, string? WikidataQid);
