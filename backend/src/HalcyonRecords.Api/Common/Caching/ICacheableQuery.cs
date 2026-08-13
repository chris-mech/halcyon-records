namespace HalcyonRecords.Api.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
}
