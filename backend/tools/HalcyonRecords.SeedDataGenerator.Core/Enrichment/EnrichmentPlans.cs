using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.Parsing;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public sealed record AddArtistPlan(
    Guid SourceId,
    string Name,
    string? Origin,
    int? ActiveSince,
    string? Bio,
    string? ImageUrl
);

public sealed record AddAlbumPlan(
    Guid SourceId,
    string Title,
    DateOnly? ReleaseDate,
    string? Label,
    IReadOnlyList<Guid> ArtistCreditIds,
    long? DiscogsMasterId,
    IReadOnlyList<DiscogsSearchResult>? DiscogsCandidates,
    IReadOnlyList<ResolvedGenre> ResolvedGenres,
    Uri? CoverImageUrl,
    string? Description,
    IReadOnlyList<AddArtistPlan> MissingArtistPlans
);
