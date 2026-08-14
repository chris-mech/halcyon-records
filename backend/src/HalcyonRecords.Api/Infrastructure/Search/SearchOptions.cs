namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public string IndexName { get; init; } = "albums";

    public int BestMatchLimit { get; init; } = 4;

    public decimal BestMatchRankingScoreThreshold { get; init; } = 0.5m;

    public int SuggestionLimit { get; init; } = 4;
}
