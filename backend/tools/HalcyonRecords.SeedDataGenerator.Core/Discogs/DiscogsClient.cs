using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public sealed class DiscogsClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public Task<DiscogsArtist?> GetArtistAsync(
        long artistId,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<DiscogsArtist>(
            $"artists/{artistId}",
            s_jsonOptions,
            cancellationToken
        );

    public Task<DiscogsMaster?> GetMasterAsync(
        long masterId,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<DiscogsMaster>(
            $"masters/{masterId}",
            s_jsonOptions,
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
            s_jsonOptions,
            cancellationToken
        );

        return result?.Results ?? [];
    }
}
