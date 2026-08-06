using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using MediatR;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed record GetAlbumsQuery(
    int Page,
    int PageSize,
    bool IsNew,
    bool IsOnSale,
    bool IsStaffPick,
    bool InStock,
    IReadOnlyList<string>? Genres,
    AlbumSortBy Sort
) : IRequest<ErrorOr<PagedResult<AlbumSummaryResponse>>>;
