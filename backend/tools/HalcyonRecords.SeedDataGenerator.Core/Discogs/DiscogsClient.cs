using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public sealed class DiscogsClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public Task<DiscogsArtist?> GetArtistAsync(
        long artistId,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<DiscogsArtist>(
            $"artists/{artistId}",
            JsonOptions,
            cancellationToken
        );

    public Task<DiscogsMaster?> GetMasterAsync(
        long masterId,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<DiscogsMaster>(
            $"masters/{masterId}",
            JsonOptions,
            cancellationToken
        );

    public async Task<IReadOnlyList<DiscogsSearchResult>> SearchMastersAsync(
        string artist,
        string releaseTitle,
        CancellationToken cancellationToken = default
    )
    {
        var requestUri =
            "database/search?type=master"
            + $"&artist={Uri.EscapeDataString(artist)}"
            + $"&release_title={Uri.EscapeDataString(releaseTitle)}";

        var result = await httpClient.GetFromJsonOrNullAsync<DiscogsSearchResponse>(
            requestUri,
            JsonOptions,
            cancellationToken
        );

        return result?.Results ?? [];
    }
}
