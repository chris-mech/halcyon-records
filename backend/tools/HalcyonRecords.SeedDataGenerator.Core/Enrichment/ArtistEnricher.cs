using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Services;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public interface IArtistEnricher
{
    Task<ErrorOr<AddArtistPlan>> PreviewAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    );
}

public sealed record AddArtistPlan(
    Guid SourceId,
    string Name,
    string? Origin,
    int? ActiveSince,
    string? Bio,
    string? ImageUrl
);

public sealed class ArtistEnricher(
    IMusicBrainzArtistService musicBrainzArtistService,
    IDiscogsArtistService discogsArtistService,
    IDescriptionService descriptionService
) : IArtistEnricher
{
    public async Task<ErrorOr<AddArtistPlan>> PreviewAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        var loaded = await musicBrainzArtistService.LoadAsync(artistMbid, cancellationToken);
        if (loaded.IsError)
        {
            return loaded.Errors;
        }

        var fields = loaded.Value;

        var discogsFields = await discogsArtistService.ResolveAsync(
            fields.DiscogsArtistId,
            cancellationToken
        );

        var bio = discogsFields.Bio;
        if (string.IsNullOrWhiteSpace(bio))
        {
            bio = await descriptionService.ResolveAsync(fields.WikidataQid, cancellationToken);
        }

        return new AddArtistPlan(
            fields.SourceId,
            fields.Name,
            fields.Origin,
            fields.ActiveSince,
            bio,
            discogsFields.ImageUrl
        );
    }
}
