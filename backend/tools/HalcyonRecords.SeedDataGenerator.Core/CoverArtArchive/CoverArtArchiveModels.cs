namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public sealed record CoverArtArchiveResponse(IReadOnlyList<CoverArtImage>? Images);

public sealed record CoverArtImage(string? Image, bool Front);
