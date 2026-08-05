using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IMusicBrainzReleaseSearchService
{
    Task<IReadOnlyList<MusicBrainzReleaseSearchResult>> ResolveAsync(
        string? artistName,
        string releaseTitle,
        CancellationToken cancellationToken = default
    );
}

public sealed class MusicBrainzReleaseSearchService(MusicBrainzClient musicBrainzClient)
    : IMusicBrainzReleaseSearchService
{
    public Task<IReadOnlyList<MusicBrainzReleaseSearchResult>> ResolveAsync(
        string? artistName,
        string releaseTitle,
        CancellationToken cancellationToken = default
    ) => musicBrainzClient.SearchReleasesAsync(artistName, releaseTitle, cancellationToken);
}
