namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed class MeilisearchIndexOptions
{
    public const string SectionName = "MeilisearchIndex";

    public string IndexName { get; init; } = "albums";
}
