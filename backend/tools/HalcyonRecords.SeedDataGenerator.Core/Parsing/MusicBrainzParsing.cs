using System.Globalization;
using ErrorOr;
using HalcyonRecords.Api.Common.Results;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

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
            ActiveSince: activeSince
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

        return new MusicBrainzReleaseFields(
            SourceId: raw.Id.Value,
            Title: raw.Title,
            ReleaseDate: releaseDate,
            Label: label,
            ArtistCreditIds: artistCreditIds,
            ReleaseGroupId: raw.ReleaseGroup?.Id
        );
    }
}
