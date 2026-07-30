using System.Threading.RateLimiting;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

public sealed class WikipediaOptions
{
    public const string SectionName = "Wikipedia";

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
