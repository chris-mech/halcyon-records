using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class GetAlbumsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums",
                async Task<
                    Results<
                        Ok<PagedResult<AlbumSummaryResponse>>,
                        ProblemHttpResult,
                        ValidationProblem
                    >
                > (
                    int? page,
                    int? pageSize,
                    bool? isNew,
                    bool? isOnSale,
                    bool? isStaffPick,
                    bool? inStock,
                    string[]? genres,
                    string? sort,
                    ISender sender
                ) =>
                {
                    ErrorOr<PagedResult<AlbumSummaryResponse>> result = await sender.Send(
                        new GetAlbumsQuery(
                            page ?? 1,
                            pageSize ?? 12,
                            isNew ?? false,
                            isOnSale ?? false,
                            isStaffPick ?? false,
                            inStock ?? false,
                            genres,
                            sort ?? nameof(AlbumSortBy.NewestFirst)
                        )
                    );

                    return result.Match<
                        Results<
                            Ok<PagedResult<AlbumSummaryResponse>>,
                            ProblemHttpResult,
                            ValidationProblem
                        >
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<PagedResult<AlbumSummaryResponse>>>()
                    );
                }
            )
            .WithName("GetAlbums");
    }
}
