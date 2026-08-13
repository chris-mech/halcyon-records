using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace HalcyonRecords.Api.Common.Behaviours;

public sealed class CachingBehaviour<TRequest, TResult>(HybridCache cache)
    : IPipelineBehavior<TRequest, ErrorOr<TResult>>
    where TRequest : IRequest<ErrorOr<TResult>>, ICacheableQuery
    where TResult : class
{
    public async Task<ErrorOr<TResult>> Handle(
        TRequest request,
        RequestHandlerDelegate<ErrorOr<TResult>> next,
        CancellationToken cancellationToken
    )
    {
        var cached = await cache.GetOrCreateAsync(
            request.CacheKey,
            async ct =>
            {
                var result = await next(ct);

                return result.IsError
                    ? new CachedResult<TResult>(
                        null,
                        result.Errors.ConvertAll(e => new CachedError(
                            e.Code,
                            e.Description,
                            e.NumericType
                        ))
                    )
                    : new CachedResult<TResult>(result.Value, null);
            },
            cancellationToken: cancellationToken
        );

        return cached.Errors is { } errors
            ? errors.ConvertAll(e => Error.Custom(e.NumericType, e.Code, e.Description))
            : cached.Value!;
    }
}
