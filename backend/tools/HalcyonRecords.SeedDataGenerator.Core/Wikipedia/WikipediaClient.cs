using System.Net.Http.Json;
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

    public async Task<WikipediaPageSummary?> GetSummaryAsync(
        string title,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetOrNullAsync(
            $"page/summary/{Uri.EscapeDataString(title)}",
            cancellationToken
        );

        return response is null
            ? null
            : await response.Content.ReadFromJsonAsync<WikipediaPageSummary>(
                JsonOptions,
                cancellationToken
            );
    }
}
