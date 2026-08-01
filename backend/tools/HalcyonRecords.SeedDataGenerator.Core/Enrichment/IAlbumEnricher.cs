using ErrorOr;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public interface IAlbumEnricher
{
    Task<ErrorOr<AddAlbumPlan>> PreviewAsync(
        Guid releaseMbid,
        IReadOnlySet<Guid> knownArtistSourceIds,
        CancellationToken cancellationToken = default
    );

    Task<AddAlbumPlan> ResolveDiscogsMasterAsync(
        AddAlbumPlan plan,
        long chosenMasterId,
        CancellationToken cancellationToken = default
    );
}
