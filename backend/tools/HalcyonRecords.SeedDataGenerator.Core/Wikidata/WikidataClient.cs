using System.Net.Http.Json;
using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public sealed class WikidataClient
{
    private const string SitelinksProps = "sitelinks";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient;

    public WikidataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WikidataEntity?> GetEntityAsync(
        string qid,
        CancellationToken cancellationToken = default
    )
    {
        var requestUri =
            $"api.php?action=wbgetentities&ids={Uri.EscapeDataString(qid)}"
            + $"&props={SitelinksProps}&sitefilter=enwiki&format=json&formatversion=2";

        var response = await _httpClient.GetOrNullAsync(requestUri, cancellationToken);

        if (response is null)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<WikidataEntitiesResponse>(
            JsonOptions,
            cancellationToken
        );

        return result?.Entities?.Values.FirstOrDefault();
    }
}
