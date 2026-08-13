using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

public sealed class WikipediaClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public Task<WikipediaPageSummary?> GetSummaryAsync(
        string title,
        CancellationToken cancellationToken = default
    ) =>
        httpClient.GetFromJsonOrNullAsync<WikipediaPageSummary>(
            $"page/summary/{Uri.EscapeDataString(title)}",
            s_jsonOptions,
            cancellationToken
        );
}
