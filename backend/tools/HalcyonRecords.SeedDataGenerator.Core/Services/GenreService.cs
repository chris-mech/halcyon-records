using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IGenreService
{
    Task<IReadOnlyList<ResolvedGenre>> ResolveAsync(
        DiscogsMasterId masterId,
        CancellationToken cancellationToken = default
    );
}

public sealed record ResolvedGenre(string Name, GenreSlug Slug);

public sealed class GenreService(DiscogsClient discogsClient) : IGenreService
{
    public static readonly IReadOnlyList<string> KnownGenreNames =
    [
        "Blues",
        "Brass & Military",
        "Children's",
        "Classical",
        "Electronic",
        "Folk, World, & Country",
        "Funk / Soul",
        "Hip-Hop",
        "Jazz",
        "Latin",
        "Non-Music",
        "Pop",
        "Reggae",
        "Rock",
        "Stage & Screen",
    ];

    public async Task<IReadOnlyList<ResolvedGenre>> ResolveAsync(
        DiscogsMasterId masterId,
        CancellationToken cancellationToken = default
    )
    {
        var master = await discogsClient.GetMasterAsync(masterId.Value, cancellationToken);

        List<ResolvedGenre> resolved = [];
        foreach (var genreName in master?.Genres ?? [])
        {
            var slug = Slugifier.Slugify(genreName);
            if (!string.IsNullOrEmpty(slug))
            {
                resolved.Add(new ResolvedGenre(genreName, new GenreSlug(slug)));
            }
        }

        return resolved;
    }
}
