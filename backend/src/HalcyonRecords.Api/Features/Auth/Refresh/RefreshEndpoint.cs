using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
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
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetResponseExample(
                        "401",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                                "title": "Unauthorized",
                                "status": 401,
                                "detail": "The refresh token is invalid, expired, or has already been used.",
                                "instance": "/api/auth/refresh",
                                "code": "Auth.InvalidRefreshToken",
                                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00"
                            }
                            """
                        )!
                    );
                    return Task.CompletedTask;
                }
            );
    }
}
