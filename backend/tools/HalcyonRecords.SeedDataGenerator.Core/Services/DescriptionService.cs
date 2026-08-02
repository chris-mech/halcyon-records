using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.Wikidata;
using HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

namespace HalcyonRecords.SeedDataGenerator.Core.Services;

public interface IDescriptionService
{
    Task<string?> ResolveAsync(WikidataQid? qid, CancellationToken cancellationToken = default);
}

public sealed class DescriptionService(
    WikidataClient wikidataClient,
    WikipediaClient wikipediaClient
) : IDescriptionService
{
    public async Task<string?> ResolveAsync(
        WikidataQid? qid,
        CancellationToken cancellationToken = default
    )
    {
        if (qid is not { } id)
        {
            return null;
        }

        var entity = await wikidataClient.GetEntityAsync(id.Value, cancellationToken);
        var title = entity?.Sitelinks?.Values.FirstOrDefault()?.Title;

        if (title is null)
        {
            return null;
        }

        var summary = await wikipediaClient.GetSummaryAsync(title, cancellationToken);
        return summary?.Extract;
    }
}
