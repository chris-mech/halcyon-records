using System.Globalization;
using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public static class MusicBrainzParsing
{
    public static ErrorOr<MusicBrainzArtistFields> ParseArtist(MusicBrainzArtist raw)
    {
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
            DiscogsArtistId: ExtractRelationId(raw.Relations, "discogs"),
            WikidataQid: ExtractRelationSlug(raw.Relations, "wikidata")
        );
    }

    public static ErrorOr<MusicBrainzReleaseFields> ParseRelease(MusicBrainzRelease raw)
    {
        if (raw.Id is null)
        {
            return DomainErrors.Album.MissingSourceId();
        }

        if (string.IsNullOrWhiteSpace(raw.Title))
        {
            return DomainErrors.Album.MissingTitle();
        }

        var releaseDate = DateOnly.TryParse(
            raw.Date,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date
        )
            ? date
            : (DateOnly?)null;

        var label = raw
            .LabelInfo?.Select(info => info.Label?.Name)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

        List<Guid> artistCreditIds =
            raw.ArtistCredit?.Select(credit => credit.Artist?.Id).OfType<Guid>().ToList() ?? [];

        var primaryArtistName = raw.ArtistCredit?.FirstOrDefault()?.Name;

        return new MusicBrainzReleaseFields(
            SourceId: raw.Id.Value,
            Title: raw.Title,
            ReleaseDate: releaseDate,
            Label: label,
            ArtistCreditIds: artistCreditIds,
            ReleaseGroupId: raw.ReleaseGroup?.Id,
            PrimaryArtistName: primaryArtistName
        );
    }

    public static MusicBrainzReleaseGroupFields ParseReleaseGroup(MusicBrainzReleaseGroup raw) =>
        new(
            DiscogsMasterId: ExtractRelationId(raw.Relations, "discogs"),
            WikidataQid: ExtractRelationSlug(raw.Relations, "wikidata")
        );

    private static long? ExtractRelationId(
        IReadOnlyList<MusicBrainzRelation>? relations,
        string type
    )
    {
        var slug = ExtractRelationSlug(relations, type);
        return long.TryParse(slug, out var id) ? id : null;
    }

    private static string? ExtractRelationSlug(
        IReadOnlyList<MusicBrainzRelation>? relations,
        string type
    )
    {
        var resource = relations?.FirstOrDefault(relation => relation.Type == type)?.Url?.Resource;
        return string.IsNullOrWhiteSpace(resource) ? null : resource.TrimEnd('/').Split('/')[^1];
    }
}
