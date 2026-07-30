using System.Net.Http.Json;
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

    public async Task<DiscogsArtist?> GetArtistAsync(
        long artistId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetOrNullAsync($"artists/{artistId}", cancellationToken);

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<DiscogsArtist>(
                JsonOptions,
                cancellationToken
            );
    }

    public async Task<DiscogsMaster?> GetMasterAsync(
        long masterId,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetOrNullAsync($"masters/{masterId}", cancellationToken);

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<DiscogsMaster>(
                JsonOptions,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<DiscogsSearchResult>> SearchMastersAsync(
        string artist,
        string releaseTitle,
        CancellationToken cancellationToken = default
    )
    {
        var requestUri =
            $"database/search?type=master"
            + $"&artist={Uri.EscapeDataString(artist)}"
            + $"&release_title={Uri.EscapeDataString(releaseTitle)}";

        var response = await _httpClient.GetOrNullAsync(requestUri, cancellationToken);

        if (response is null)
        {
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<DiscogsSearchResponse>(
            JsonOptions,
            cancellationToken
        );

        return result?.Results ?? [];
    }
}
