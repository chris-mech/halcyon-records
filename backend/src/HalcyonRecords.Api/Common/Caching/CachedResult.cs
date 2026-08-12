namespace HalcyonRecords.Api.Common.Caching;

internal sealed record CachedError(string Code, string Description, int NumericType);

internal sealed record CachedResult<TResult>(TResult? Value, List<CachedError>? Errors)
    where TResult : class;
