using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed class GetAlbumByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/{sqid}",
                async Task<Results<Ok<AlbumDetailResponse>, ProblemHttpResult, ValidationProblem>> (
                    string sqid,
                    ISender sender
                ) =>
                {
                    ErrorOr<AlbumDetailResponse> result = await sender.Send(
                        new GetAlbumByIdQuery(sqid)
                    );

                    return result.Match<
                        Results<Ok<AlbumDetailResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<AlbumDetailResponse>>()
                    );
                }
            )
            .WithName("GetAlbumById");
    }
}
