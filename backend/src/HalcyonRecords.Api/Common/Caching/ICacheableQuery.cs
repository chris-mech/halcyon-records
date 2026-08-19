namespace HalcyonRecords.Api.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    IReadOnlyCollection<string> Tags { get; }
}
