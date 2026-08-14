using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed record GetDecadeBySlugQuery(string Slug)
    : IRequest<ErrorOr<DecadeDetailResponse>>,
        ICacheableQuery
{
    public string CacheKey => $"decades:bySlug:{Slug}";
}
