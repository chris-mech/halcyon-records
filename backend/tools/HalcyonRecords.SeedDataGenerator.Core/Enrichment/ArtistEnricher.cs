using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Parsing;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public sealed class ArtistEnricher(
    MusicBrainzClient musicBrainzClient,
    DiscogsClient discogsClient,
    WikipediaDescriptionResolver descriptionResolver
) : IArtistEnricher
{
    public async Task<ErrorOr<AddArtistPlan>> PreviewAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetArtistAsync(artistMbid, cancellationToken);
        if (raw is null)
        {
            return DomainErrors.Artist.NotFound($"No MusicBrainz artist found for '{artistMbid}'.");
        }

        var parsed = MusicBrainzParsing.ParseArtist(raw);
        if (parsed.IsError)
        {
            return parsed.Errors;
        }

        var fields = parsed.Value;

        var discogsArtist = fields.DiscogsArtistId is not null
            ? await discogsClient.GetArtistAsync(fields.DiscogsArtistId.Value, cancellationToken)
            : null;
        var discogsFields = DiscogsParsing.ParseArtist(discogsArtist);

        var bio = discogsFields.Bio;
        if (string.IsNullOrWhiteSpace(bio))
        {
            bio = await descriptionResolver.ResolveAsync(fields.WikidataQid, cancellationToken);
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
