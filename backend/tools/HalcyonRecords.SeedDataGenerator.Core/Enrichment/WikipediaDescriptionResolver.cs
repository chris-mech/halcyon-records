using HalcyonRecords.SeedDataGenerator.Core.Parsing;
using HalcyonRecords.SeedDataGenerator.Core.Wikidata;
using HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public sealed class WikipediaDescriptionResolver(
    WikidataClient wikidataClient,
    WikipediaClient wikipediaClient
)
{
    public async Task<string?> ResolveAsync(
        string? qid,
        CancellationToken cancellationToken = default
    )
    {
        if (qid is null)
        {
            return null;
        }

        var entity = await wikidataClient.GetEntityAsync(qid, cancellationToken);
        var title = WikidataParsing.ParseSitelinkTitle(entity);

        if (title is null)
        {
            return null;
        }

        var summary = await wikipediaClient.GetSummaryAsync(title, cancellationToken);
        return WikipediaParsing.ParseExtract(summary);
    }
}
