using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetCoverStory;

public sealed class GetCoverStoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/cover-story",
                async Task<Results<Ok<CoverStoryResponse>, ProblemHttpResult>> (ISender sender) =>
                {
                    ErrorOr<CoverStoryResponse> result = await sender.Send(
                        new GetCoverStoryQuery()
                    );

                    return result.Match<Results<Ok<CoverStoryResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<CoverStoryResponse>>()
                    );
                }
            )
            .WithName("GetCoverStory")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
