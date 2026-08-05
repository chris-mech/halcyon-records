using System.Threading.RateLimiting;

namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public sealed class CoverArtArchiveOptions
{
    public const string SectionName = "CoverArtArchive";

    public string BaseAddress { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public TokenBucketRateLimiterOptions RateLimit { get; set; } =
        new()
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        };
}
