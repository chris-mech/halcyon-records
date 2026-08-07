using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed record GetAlbumByIdQuery(string Sqid) : IRequest<ErrorOr<AlbumDetailResponse>>;
