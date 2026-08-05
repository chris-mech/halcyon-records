using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IMusicBrainzArtistSearchService
{
    Task<IReadOnlyList<MusicBrainzArtistSearchResult>> ResolveAsync(
        string name,
        CancellationToken cancellationToken = default
    );
}

public sealed class MusicBrainzArtistSearchService(MusicBrainzClient musicBrainzClient)
    : IMusicBrainzArtistSearchService
{
    public Task<IReadOnlyList<MusicBrainzArtistSearchResult>> ResolveAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => musicBrainzClient.SearchArtistsAsync(name, cancellationToken);
}
