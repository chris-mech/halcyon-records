using System.Net.Http.Json;
using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

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
        var response = await _httpClient.GetOrNullAsync($"release/{mbid}/", cancellationToken);

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
        var response = await _httpClient.GetOrNullAsync(
            $"release-group/{mbid}/",
            cancellationToken
        );

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<CoverArtArchiveResponse>(
                JsonOptions,
                cancellationToken
            );
    }
}
