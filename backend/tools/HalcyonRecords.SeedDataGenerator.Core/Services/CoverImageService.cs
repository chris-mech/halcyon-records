using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface ICoverImageService
{
    Task<Uri?> ResolveAsync(
        ReleaseMbid releaseMbid,
        ReleaseGroupMbid? releaseGroupId,
        CancellationToken cancellationToken = default
    );
}

public sealed class CoverImageService(CoverArtArchiveClient coverArtArchiveClient)
    : ICoverImageService
{
    public async Task<Uri?> ResolveAsync(
        ReleaseMbid releaseMbid,
        ReleaseGroupMbid? releaseGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var coverImageUrl = await coverArtArchiveClient.GetReleaseFrontImageUrlAsync(
            releaseMbid.Value,
            cancellationToken
        );
        if (coverImageUrl is not null || releaseGroupId is not { } fallbackGroupId)
        {
            return coverImageUrl;
        }

        return await coverArtArchiveClient.GetReleaseGroupFrontImageUrlAsync(
            fallbackGroupId.Value,
            cancellationToken
        );
    }
}
