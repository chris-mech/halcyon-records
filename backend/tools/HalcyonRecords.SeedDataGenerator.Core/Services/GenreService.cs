using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IGenreService
{
    Task<IReadOnlyList<ResolvedGenre>> ResolveAsync(
        long masterId,
        CancellationToken cancellationToken = default
    );
}

public sealed record ResolvedGenre(string Name, string Slug);

public sealed class GenreService(DiscogsClient discogsClient) : IGenreService
{
    public async Task<IReadOnlyList<ResolvedGenre>> ResolveAsync(
        long masterId,
        CancellationToken cancellationToken = default
    )
    {
        var master = await discogsClient.GetMasterAsync(masterId, cancellationToken);

        List<ResolvedGenre> resolved = [];
        foreach (var genreName in master?.Genres ?? [])
        {
            var slug = Slugifier.Slugify(genreName);
            if (!string.IsNullOrEmpty(slug))
            {
                resolved.Add(new ResolvedGenre(genreName, slug));
            }
        }

        return resolved;
    }
}
