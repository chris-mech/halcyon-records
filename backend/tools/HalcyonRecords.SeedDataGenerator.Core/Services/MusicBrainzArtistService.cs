using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IMusicBrainzArtistService
{
    Task<ErrorOr<MusicBrainzArtistFields>> LoadAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    );
}

public sealed record MusicBrainzArtistFields(
    Guid SourceId,
    string Name,
    string? Origin,
    int? ActiveSince,
    long? DiscogsArtistId,
    string? WikidataQid
);

public sealed class MusicBrainzArtistService(MusicBrainzClient musicBrainzClient)
    : IMusicBrainzArtistService
{
    public async Task<ErrorOr<MusicBrainzArtistFields>> LoadAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetArtistAsync(artistMbid, cancellationToken);
        if (raw is null)
        {
            return DomainErrors.Artist.NotFound($"No MusicBrainz artist found for '{artistMbid}'.");
        }

        if (raw.Id is null)
        {
            return DomainErrors.Artist.MissingSourceId();
        }

        if (string.IsNullOrWhiteSpace(raw.Name))
        {
            return DomainErrors.Artist.MissingName();
        }

        var activeSince = int.TryParse(raw.LifeSpan?.Begin, out var year) ? year : (int?)null;

        return new MusicBrainzArtistFields(
            SourceId: raw.Id.Value,
            Name: raw.Name,
            Origin: raw.Area?.Name,
            ActiveSince: activeSince,
            DiscogsArtistId: MusicBrainzRelations.ExtractId(raw.Relations, "discogs"),
            WikidataQid: MusicBrainzRelations.ExtractSlug(raw.Relations, "wikidata")
        );
    }
}
