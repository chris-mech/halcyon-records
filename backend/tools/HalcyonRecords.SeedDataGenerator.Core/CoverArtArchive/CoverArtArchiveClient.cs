using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public sealed class CoverArtArchiveClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient;

    public CoverArtArchiveClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CoverArtArchiveResponse?> GetByReleaseAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    )
    {
        var response = await SendAsync($"release/{mbid}/", cancellationToken);

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<CoverArtArchiveResponse>(
                JsonOptions,
                cancellationToken
            );
    }

    public async Task<CoverArtArchiveResponse?> GetByReleaseGroupAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    )
    {
        var response = await SendAsync($"release-group/{mbid}/", cancellationToken);

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<CoverArtArchiveResponse>(
                JsonOptions,
                cancellationToken
            );
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
