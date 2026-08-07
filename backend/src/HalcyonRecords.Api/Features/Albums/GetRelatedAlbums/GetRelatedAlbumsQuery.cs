using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed record GetRelatedAlbumsQuery(string Sqid)
    : IRequest<ErrorOr<IReadOnlyList<RelatedAlbumResponse>>>;
