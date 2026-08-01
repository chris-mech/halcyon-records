using ErrorOr;
using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Results;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;

namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public sealed record ResolvedGenre(string Name, string Slug);

public sealed record DiscogsArtistFields(string? Bio, string? ImageUrl);

public static class DiscogsParsing
{
    public static ErrorOr<string> ParseGenre(string discogsGenreName)
    {
        var slug = Slugifier.Slugify(discogsGenreName);

        return string.IsNullOrEmpty(slug) ? DomainErrors.Genre.EmptySlug(discogsGenreName) : slug;
    }

    public static DiscogsArtistFields ParseArtist(DiscogsArtist? raw) =>
        new(
            Bio: raw?.Profile,
            ImageUrl: raw?.Images?.FirstOrDefault(image => image.Type == "primary")?.Uri
                ?? raw?.Images?.FirstOrDefault()?.Uri
        );

    public static IReadOnlyList<ResolvedGenre> ParseMasterGenres(DiscogsMaster? raw)
    {
        List<ResolvedGenre> resolved = [];

        foreach (var genreName in raw?.Genres ?? [])
        {
            var parsed = ParseGenre(genreName);
            if (!parsed.IsError)
            {
                resolved.Add(new ResolvedGenre(genreName, parsed.Value));
            }
        }

        return resolved;
    }
}
