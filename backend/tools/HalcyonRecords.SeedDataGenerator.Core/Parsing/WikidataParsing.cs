using HalcyonRecords.SeedDataGenerator.Core.Wikidata;

namespace HalcyonRecords.SeedDataGenerator.Core.Parsing;

public static class WikidataParsing
{
    public static string? ParseSitelinkTitle(WikidataEntity? entity) =>
        entity?.Sitelinks?.Values.FirstOrDefault()?.Title;
}
