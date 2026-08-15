namespace HalcyonRecords.Api.Features.Search.Search;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public decimal BestMatchRankingScoreThreshold { get; init; } = 0.5m;

    public int SuggestionLimit { get; init; } = 4;

    public int SuggestedTermCount { get; init; } = 3;
}
