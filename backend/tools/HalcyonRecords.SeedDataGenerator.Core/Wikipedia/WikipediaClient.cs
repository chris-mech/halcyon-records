using System.Text.Json;
using HalcyonRecords.SeedDataGenerator.Core.Common;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

public sealed class WikipediaClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient;

    public WikipediaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<WikipediaPageSummary?> GetSummaryAsync(
        string title,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.GetFromJsonOrNullAsync<WikipediaPageSummary>(
            $"page/summary/{Uri.EscapeDataString(title)}",
            JsonOptions,
            cancellationToken
        );
}
