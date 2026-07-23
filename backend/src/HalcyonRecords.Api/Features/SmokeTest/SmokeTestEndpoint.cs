using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.SmokeTest;

public sealed class SmokeTestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/smoke-test",
            async Task<Results<Ok<SmokeTestResponse>, ProblemHttpResult, ValidationProblem>> (
                bool shouldFail,
                ISender sender
            ) =>
            {
                ErrorOr<SmokeTestResponse> result = await sender.Send(
                    new SmokeTestQuery(shouldFail)
                );

                return result.Match<
                    Results<Ok<SmokeTestResponse>, ProblemHttpResult, ValidationProblem>
                >(
                    response => TypedResults.Ok(response),
                    errors => errors.Problem<Ok<SmokeTestResponse>>()
                );
            }
        );
    }
}
