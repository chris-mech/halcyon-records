namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public string IndexName { get; init; } = "albums";

    public int SuggestionLimit { get; init; } = 4;
}
