using System.Threading.RateLimiting;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public sealed class WikidataOptions
{
    public const string SectionName = "Wikidata";

    public string BaseAddress { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public SlidingWindowRateLimiterOptions RateLimit { get; set; } =
        new()
        {
            PermitLimit = 200,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            AutoReplenishment = true,
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        };
}
