using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Genres.GetGenres;

public sealed record GetGenresQuery
    : IRequest<ErrorOr<IReadOnlyList<GenreListItemResponse>>>,
        ICacheableQuery
{
    public string CacheKey => "genres:list";

    public IReadOnlyCollection<string> Tags => [];
}
