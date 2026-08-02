using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IReleaseGroupService
{
    Task<MusicBrainzReleaseGroupFields?> ResolveAsync(
        Guid releaseGroupId,
        CancellationToken cancellationToken = default
    );
}

public sealed record MusicBrainzReleaseGroupFields(long? DiscogsMasterId, string? WikidataQid);

public sealed class ReleaseGroupService(MusicBrainzClient musicBrainzClient) : IReleaseGroupService
{
    public async Task<MusicBrainzReleaseGroupFields?> ResolveAsync(
        Guid releaseGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetReleaseGroupAsync(releaseGroupId, cancellationToken);
        return raw is null
            ? null
            : new MusicBrainzReleaseGroupFields(
                DiscogsMasterId: MusicBrainzRelations.ExtractId(raw.Relations, "discogs"),
                WikidataQid: MusicBrainzRelations.ExtractSlug(raw.Relations, "wikidata")
            );
    }
}
