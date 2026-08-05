using System.Threading.RateLimiting;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public sealed class DiscogsOptions
{
    public const string SectionName = "Discogs";

    public string BaseAddress { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public SlidingWindowRateLimiterOptions RateLimit { get; set; } =
        new()
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            AutoReplenishment = true,
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        };
}
