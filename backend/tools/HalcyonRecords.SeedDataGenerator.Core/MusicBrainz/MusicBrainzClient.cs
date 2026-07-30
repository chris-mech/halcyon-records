using System.Net.Http.Json;
using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public sealed class MusicBrainzClient
{
    private const string ArtistIncludes = "url-rels";
    private const string ReleaseIncludes = "artist-credits+labels+genres+release-groups";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    };

    private readonly HttpClient _httpClient;

    public MusicBrainzClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MusicBrainzArtist?> GetArtistAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetOrNullAsync(
            $"artist/{mbid}?inc={ArtistIncludes}&fmt=json",
            cancellationToken
        );

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<MusicBrainzArtist>(
                JsonOptions,
                cancellationToken
            );
    }

    public async Task<MusicBrainzRelease?> GetReleaseAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetOrNullAsync(
            $"release/{mbid}?inc={ReleaseIncludes}&fmt=json",
            cancellationToken
        );

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<MusicBrainzRelease>(
                JsonOptions,
                cancellationToken
            );
    }
}
