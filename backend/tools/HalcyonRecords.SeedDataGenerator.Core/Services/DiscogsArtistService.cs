using HalcyonRecords.SeedDataGenerator.Core.Discogs;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IDiscogsArtistService
{
    Task<DiscogsArtistFields> ResolveAsync(
        long? discogsArtistId,
        CancellationToken cancellationToken = default
    );
}

public sealed record DiscogsArtistFields(string? Bio, string? ImageUrl);

public sealed class DiscogsArtistService(DiscogsClient discogsClient) : IDiscogsArtistService
{
    public async Task<DiscogsArtistFields> ResolveAsync(
        long? discogsArtistId,
        CancellationToken cancellationToken = default
    )
    {
        var raw = discogsArtistId is { } id
            ? await discogsClient.GetArtistAsync(id, cancellationToken)
            : null;

        return new DiscogsArtistFields(
            Bio: raw?.Profile,
            ImageUrl: raw?.Images?.FirstOrDefault(image => image.Type == "primary")?.Uri
                ?? raw?.Images?.FirstOrDefault()?.Uri
        );
    }
}
