using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed class RefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/refresh",
                async Task<Results<Ok<RefreshResponse>, ProblemHttpResult, ValidationProblem>> (
                    RefreshRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<RefreshResponse> result = await sender.Send(
                        new RefreshCommand(request.RefreshToken)
                    );

                    return result.Match<
                        Results<Ok<RefreshResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<RefreshResponse>>()
                    );
                }
            )
            .WithName("Refresh");
    }
}
