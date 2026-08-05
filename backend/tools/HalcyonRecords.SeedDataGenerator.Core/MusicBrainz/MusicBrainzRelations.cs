namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

internal static class MusicBrainzRelations
{
    public static long? ExtractId(IReadOnlyList<MusicBrainzRelation>? relations, string type)
    {
        var slug = ExtractSlug(relations, type);
        return long.TryParse(slug, out var id) ? id : null;
    }

    public static string? ExtractSlug(IReadOnlyList<MusicBrainzRelation>? relations, string type)
    {
        var resource = relations?.FirstOrDefault(relation => relation.Type == type)?.Url?.Resource;
        return string.IsNullOrWhiteSpace(resource) ? null : resource.TrimEnd('/').Split('/')[^1];
    }
}
