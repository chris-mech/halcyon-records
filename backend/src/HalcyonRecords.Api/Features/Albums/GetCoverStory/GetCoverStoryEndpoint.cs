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
            .WithTags("Albums")
            .WithSummary("Get this week's featured staff-pick album.")
            .WithDescription(
                "Rotates deterministically through staff-picked albums on a weekly cycle. If no "
                    + "albums are flagged as staff picks, falls back to the newest release instead. "
                    + "Returns 404 only if the catalogue has no albums at all."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            );
    }
}
