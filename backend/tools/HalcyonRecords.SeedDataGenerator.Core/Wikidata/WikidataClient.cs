using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public sealed class WikidataClient(HttpClient httpClient)
{
    private const string SitelinksProps = "sitelinks";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<WikidataEntity?> GetEntityAsync(
        string qid,
        CancellationToken cancellationToken = default
    )
    {
        var requestUri =
            $"api.php?action=wbgetentities&ids={Uri.EscapeDataString(qid)}"
            + $"&props={SitelinksProps}&sitefilter=enwiki&format=json&formatversion=2";

        var result = await httpClient.GetFromJsonOrNullAsync<WikidataEntitiesResponse>(
            requestUri,
            JsonOptions,
            cancellationToken
        );

        return result?.Entities?.Values.FirstOrDefault();
    }
}
