using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public sealed class DiscogsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient _httpClient;

    public DiscogsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<DiscogsArtist?> GetArtistAsync(
        long artistId,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<DiscogsArtist>(
            $"artists/{artistId}",
            JsonOptions,
            cancellationToken
        );

    public Task<DiscogsMaster?> GetMasterAsync(
        long masterId,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<DiscogsMaster>(
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

        var result = await _httpClient.GetFromJsonOrNullAsync<DiscogsSearchResponse>(
            requestUri,
            JsonOptions,
            cancellationToken
        );

        return result?.Results ?? [];
    }
}
