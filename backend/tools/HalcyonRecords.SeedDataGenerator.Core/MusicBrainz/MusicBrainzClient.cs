using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public sealed class MusicBrainzClient
{
    private const string ArtistIncludes = "url-rels";
    private const string ReleaseIncludes = "artist-credits+labels+release-groups";
    private const string ReleaseGroupIncludes = "url-rels";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    };

    private readonly HttpClient _httpClient;

    public MusicBrainzClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<MusicBrainzArtist?> GetArtistAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<MusicBrainzArtist>(
            $"artist/{mbid}?inc={ArtistIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );

    public Task<MusicBrainzRelease?> GetReleaseAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<MusicBrainzRelease>(
            $"release/{mbid}?inc={ReleaseIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );

    public Task<MusicBrainzReleaseGroup?> GetReleaseGroupAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<MusicBrainzReleaseGroup>(
            $"release-group/{mbid}?inc={ReleaseGroupIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );
}
