using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Decades.GetDecades;

public sealed record GetDecadesQuery
    : IRequest<ErrorOr<IReadOnlyList<DecadeListItemResponse>>>,
        ICacheableQuery
{
    public string CacheKey => "decades:list";
}
