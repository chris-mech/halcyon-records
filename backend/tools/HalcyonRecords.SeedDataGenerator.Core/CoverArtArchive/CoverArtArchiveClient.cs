using System.Net;

namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public sealed class CoverArtArchiveClient
{
    private readonly HttpClient _httpClient;

    public CoverArtArchiveClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<Uri?> GetReleaseFrontImageUrlAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) => GetFrontImageUrlAsync($"release/{mbid}/front", cancellationToken);

    public Task<Uri?> GetReleaseGroupFrontImageUrlAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) => GetFrontImageUrlAsync($"release-group/{mbid}/front", cancellationToken);

    private async Task<Uri?> GetFrontImageUrlAsync(
        string requestUri,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
        {
            return response.Headers.Location;
        }

        response.EnsureSuccessStatusCode();
        return response.Headers.Location;
    }
}
