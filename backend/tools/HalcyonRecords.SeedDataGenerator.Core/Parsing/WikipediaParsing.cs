using HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public static class WikipediaParsing
{
    public static string? ParseExtract(WikipediaPageSummary? summary) => summary?.Extract;
}
