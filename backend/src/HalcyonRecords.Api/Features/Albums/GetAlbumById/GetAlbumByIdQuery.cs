using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed record GetAlbumByIdQuery(string Sqid)
    : IRequest<ErrorOr<AlbumDetailResponse>>,
        ICacheableQuery
{
    public string CacheKey => $"albums:byId:{Sqid}";
}
