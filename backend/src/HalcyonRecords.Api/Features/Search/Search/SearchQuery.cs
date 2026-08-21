using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Search.Search;

public sealed record SearchQuery(string Q) : IRequest<ErrorOr<SearchResponse>>, ICacheableQuery
{
    public string CacheKey => $"search:q={Q.Trim().ToLowerInvariant()}";

    public IReadOnlyCollection<string> Tags => ["albums"];
}
