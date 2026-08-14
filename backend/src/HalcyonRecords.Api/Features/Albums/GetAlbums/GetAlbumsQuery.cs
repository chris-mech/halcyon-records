using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
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
    int? StartYear,
    int? EndYear,
    string Sort
) : IRequest<ErrorOr<PagedResult<AlbumSummaryResponse>>>, ICacheableQuery
{
    public string CacheKey =>
        $"albums:list"
        + $":p={Page}"
        + $":ps={PageSize}"
        + $":new={IsNew}"
        + $":sale={IsOnSale}"
        + $":staff={IsStaffPick}"
        + $":stock={InStock}"
        + $":sort={Sort}"
        + $":genres={(Genres is { Count: > 0 } 
            ? string.Join(',', Genres.Order(StringComparer.Ordinal)) 
            : string.Empty)}"
        + $":start={StartYear}"
        + $":end={EndYear}";
}
