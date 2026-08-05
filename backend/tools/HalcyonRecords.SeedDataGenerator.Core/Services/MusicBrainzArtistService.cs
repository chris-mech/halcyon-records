using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IMusicBrainzArtistService
{
    Task<ErrorOr<MusicBrainzArtistFields>> LoadAsync(
        ArtistMbid artistMbid,
        CancellationToken cancellationToken = default
    );
}

public sealed record MusicBrainzArtistFields(
    ArtistMbid SourceId,
    string Name,
    string? Origin,
    ArtistType? Type,
    int? SinceYear,
    DiscogsArtistId? DiscogsArtistId,
    WikidataQid? WikidataQid
);

public sealed class MusicBrainzArtistService(MusicBrainzClient musicBrainzClient)
    : IMusicBrainzArtistService
{
    public async Task<ErrorOr<MusicBrainzArtistFields>> LoadAsync(
        ArtistMbid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetArtistAsync(artistMbid.Value, cancellationToken);
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

        var sinceYear =
            raw.LifeSpan?.Begin is { Length: >= 4 } begin
            && int.TryParse(begin.AsSpan(0, 4), out var year)
                ? year
                : (int?)null;

        var type = Enum.TryParse<ArtistType>(raw.Type, ignoreCase: true, out var parsedType)
            ? parsedType
            : (ArtistType?)null;

        return new MusicBrainzArtistFields(
            SourceId: new ArtistMbid(raw.Id.Value),
            Name: raw.Name,
            Origin: raw.Area?.Name,
            Type: type,
            SinceYear: sinceYear,
            DiscogsArtistId: MusicBrainzRelations.ExtractId(raw.Relations, "discogs")
                is { } discogsId
                ? new DiscogsArtistId(discogsId)
                : null,
            WikidataQid: MusicBrainzRelations.ExtractSlug(raw.Relations, "wikidata")
                is { } wikidataQid
                ? new WikidataQid(wikidataQid)
                : null
        );
    }
}
