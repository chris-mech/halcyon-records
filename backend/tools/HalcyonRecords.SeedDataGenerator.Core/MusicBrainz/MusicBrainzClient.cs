using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public sealed class MusicBrainzClient(HttpClient httpClient)
{
    private const string ArtistIncludes = "url-rels";
    private const string ReleaseIncludes = "artist-credits+labels+release-groups";
    private const string ReleaseGroupIncludes = "url-rels";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    };

    public Task<MusicBrainzArtist?> GetArtistAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<MusicBrainzArtist>(
            $"artist/{mbid}?inc={ArtistIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );

    public Task<MusicBrainzRelease?> GetReleaseAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<MusicBrainzRelease>(
            $"release/{mbid}?inc={ReleaseIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );

    public Task<MusicBrainzReleaseGroup?> GetReleaseGroupAsync(
        Guid mbid,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<MusicBrainzReleaseGroup>(
            $"release-group/{mbid}?inc={ReleaseGroupIncludes}&fmt=json",
            JsonOptions,
            cancellationToken
        );

    public async Task<IReadOnlyList<MusicBrainzArtistSearchResult>> SearchArtistsAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var query = $"artist:{EscapeLuceneTerm(name)}";

        var result = await httpClient.GetFromJsonOrNullAsync<MusicBrainzArtistSearchResponse>(
            $"artist?query={Uri.EscapeDataString(query)}&fmt=json",
            JsonOptions,
            cancellationToken
        );

        return result?.Artists ?? [];
    }

    public async Task<IReadOnlyList<MusicBrainzReleaseSearchResult>> SearchReleasesAsync(
        string? artist,
        string releaseTitle,
        CancellationToken cancellationToken = default
    )
    {
        var query = artist is not null
            ? $"release:{EscapeLuceneTerm(releaseTitle)} AND artist:{EscapeLuceneTerm(artist)}"
            : $"release:{EscapeLuceneTerm(releaseTitle)}";

        var result = await httpClient.GetFromJsonOrNullAsync<MusicBrainzReleaseSearchResponse>(
            $"release?query={Uri.EscapeDataString(query)}&fmt=json",
            JsonOptions,
            cancellationToken
        );

        return result?.Releases ?? [];
    }

    private static string EscapeLuceneTerm(string value) =>
        $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
}
