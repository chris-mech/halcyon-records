using HalcyonRecords.SeedDataGenerator.Core.Discogs;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IDiscogsMasterSearchService
{
    Task<IReadOnlyList<DiscogsSearchResult>> ResolveAsync(
        string? primaryArtistName,
        string releaseTitle,
        CancellationToken cancellationToken = default
    );
}

public sealed class DiscogsMasterSearchService(DiscogsClient discogsClient)
    : IDiscogsMasterSearchService
{
    public Task<IReadOnlyList<DiscogsSearchResult>> ResolveAsync(
        string? primaryArtistName,
        string releaseTitle,
        CancellationToken cancellationToken = default
    ) =>
        primaryArtistName is not null
            ? discogsClient.SearchMastersAsync(primaryArtistName, releaseTitle, cancellationToken)
            : Task.FromResult<IReadOnlyList<DiscogsSearchResult>>([]);
}
