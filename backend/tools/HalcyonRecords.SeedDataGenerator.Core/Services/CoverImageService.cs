using HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface ICoverImageService
{
    Task<Uri?> ResolveAsync(
        Guid releaseMbid,
        Guid? releaseGroupId,
        CancellationToken cancellationToken = default
    );
}

public sealed class CoverImageService(CoverArtArchiveClient coverArtArchiveClient)
    : ICoverImageService
{
    public async Task<Uri?> ResolveAsync(
        Guid releaseMbid,
        Guid? releaseGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var coverImageUrl = await coverArtArchiveClient.GetReleaseFrontImageUrlAsync(
            releaseMbid,
            cancellationToken
        );
        if (coverImageUrl is not null || releaseGroupId is not { } fallbackGroupId)
        {
            return coverImageUrl;
        }

        return await coverArtArchiveClient.GetReleaseGroupFrontImageUrlAsync(
            fallbackGroupId,
            cancellationToken
        );
    }
}
