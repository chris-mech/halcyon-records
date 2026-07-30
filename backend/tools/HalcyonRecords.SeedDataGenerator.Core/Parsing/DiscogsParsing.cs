using ErrorOr;
using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Results;

namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public static class DiscogsParsing
{
    public static ErrorOr<string> ParseGenre(string discogsGenreName)
    {
        var slug = Slugifier.Slugify(discogsGenreName);

        return string.IsNullOrEmpty(slug) ? DomainErrors.Genre.EmptySlug(discogsGenreName) : slug;
    }
}
