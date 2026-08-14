using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Genres.GetGenreBySlug;

public sealed record GetGenreBySlugQuery(string Slug)
    : IRequest<ErrorOr<GenreDetailResponse>>,
        ICacheableQuery
{
    public string CacheKey => $"genres:bySlug:{Slug}";
}
