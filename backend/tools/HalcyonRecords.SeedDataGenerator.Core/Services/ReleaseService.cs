using System.Globalization;
using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IReleaseService
{
    Task<ErrorOr<MusicBrainzReleaseFields>> LoadAsync(
        Guid releaseMbid,
        CancellationToken cancellationToken = default
    );
}

public sealed record MusicBrainzReleaseFields(
    Guid SourceId,
    string Title,
    DateOnly? ReleaseDate,
    string? Label,
    IReadOnlyList<Guid> ArtistCreditIds,
    Guid? ReleaseGroupId,
    string? PrimaryArtistName
);

public sealed class ReleaseService(MusicBrainzClient musicBrainzClient) : IReleaseService
{
    public async Task<ErrorOr<MusicBrainzReleaseFields>> LoadAsync(
        Guid releaseMbid,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetReleaseAsync(releaseMbid, cancellationToken);
        if (raw is null)
        {
            return DomainErrors.Album.NotFound(
                $"No MusicBrainz release found for '{releaseMbid}'."
            );
        }

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
            ReleaseGroupId: raw.ReleaseGroup?.Id,
            PrimaryArtistName: raw.ArtistCredit?.FirstOrDefault()?.Name
        );
    }
}
