namespace HalcyonRecords.Api.Common.Caching;

public sealed class CachingOptions
{
    public const string SectionName = "Caching";

    public int TtlSeconds { get; init; } = 60;
}
