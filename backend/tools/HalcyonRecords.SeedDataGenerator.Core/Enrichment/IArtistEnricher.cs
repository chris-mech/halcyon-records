using ErrorOr;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public interface IArtistEnricher
{
    Task<ErrorOr<AddArtistPlan>> PreviewAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    );
}
