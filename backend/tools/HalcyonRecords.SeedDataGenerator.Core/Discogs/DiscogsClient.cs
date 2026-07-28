using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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
        var response = await SendAsync($"artists/{artistId}", cancellationToken);

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
        var response = await SendAsync($"masters/{masterId}", cancellationToken);

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

        var response = await SendAsync(requestUri, cancellationToken);

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

    private async Task<HttpResponseMessage?> SendAsync(
        string requestUri,
        CancellationToken cancellationToken
    )
    {
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return response;
    }
}
