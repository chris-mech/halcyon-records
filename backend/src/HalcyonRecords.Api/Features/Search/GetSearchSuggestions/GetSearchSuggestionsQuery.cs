using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Search.GetSearchSuggestions;

public sealed record GetSearchSuggestionsQuery
    : IRequest<ErrorOr<IReadOnlyList<string>>>,
        ICacheableQuery
{
    public string CacheKey => "search:suggestions";
}
