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
            .WithName("Refresh")
            .WithTags("Auth")
            .WithSummary("Exchange a valid refresh token for a new access token and refresh token.")
            .WithDescription(
                "Rotates the refresh token: the given token is revoked and a new one is issued. "
                    + "Reusing an already-revoked token is treated as a compromise and revokes its "
                    + "entire descendant chain, invalidating every token issued after it."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status401Unauthorized,
                "application/problem+json"
            );
    }
}
