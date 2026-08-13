using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetCoverStory;

public sealed record GetCoverStoryQuery : IRequest<ErrorOr<CoverStoryResponse>>, ICacheableQuery
{
    public string CacheKey => "albums:cover-story";
}
