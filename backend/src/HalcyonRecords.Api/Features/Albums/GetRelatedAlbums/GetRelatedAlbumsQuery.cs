using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed record GetRelatedAlbumsQuery(string Sqid)
    : IRequest<ErrorOr<IReadOnlyList<RelatedAlbumResponse>>>,
        ICacheableQuery
{
    public string CacheKey => $"albums:related:{Sqid}";
}
