using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IReleaseGroupService
{
    Task<MusicBrainzReleaseGroupFields?> ResolveAsync(
        ReleaseGroupMbid releaseGroupId,
        CancellationToken cancellationToken = default
    );
}

public sealed record MusicBrainzReleaseGroupFields(
    DiscogsMasterId? DiscogsMasterId,
    WikidataQid? WikidataQid
);

public sealed class ReleaseGroupService(MusicBrainzClient musicBrainzClient) : IReleaseGroupService
{
    public async Task<MusicBrainzReleaseGroupFields?> ResolveAsync(
        ReleaseGroupMbid releaseGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetReleaseGroupAsync(
            releaseGroupId.Value,
            cancellationToken
        );

        if (raw is null)
        {
            return null;
        }

        DiscogsMasterId? discogsMasterId = MusicBrainzRelations.ExtractId(raw.Relations, "discogs")
            is { } discogsId
            ? new DiscogsMasterId(discogsId)
            : null;

        WikidataQid? wikidataQid = MusicBrainzRelations.ExtractSlug(raw.Relations, "wikidata")
            is { } wikidataSlug
            ? new WikidataQid(wikidataSlug)
            : null;

        return new MusicBrainzReleaseGroupFields(discogsMasterId, wikidataQid);
    }
}
