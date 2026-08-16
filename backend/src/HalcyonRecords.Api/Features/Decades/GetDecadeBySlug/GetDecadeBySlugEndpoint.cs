using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed class GetDecadeBySlugEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/decades/{slug}",
                async Task<Results<Ok<DecadeDetailResponse>, ProblemHttpResult>> (
                    string slug,
                    ISender sender
                ) =>
                {
                    ErrorOr<DecadeDetailResponse> result = await sender.Send(
                        new GetDecadeBySlugQuery(slug)
                    );

                    return result.Match<Results<Ok<DecadeDetailResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<DecadeDetailResponse>>()
                    );
                }
            )
            .WithName("GetDecadeBySlug")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
