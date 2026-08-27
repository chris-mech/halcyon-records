using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/login",
                async Task<Results<Ok<LoginResponse>, ProblemHttpResult, ValidationProblem>> (
                    LoginRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<LoginResponse> result = await sender.Send(
                        new LoginCommand(request.Email, request.Password)
                    );

                    return result.Match<
                        Results<Ok<LoginResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<LoginResponse>>()
                    );
                }
            )
            .WithName("Login")
            .WithTags("Auth")
            .WithSummary(
                "Authenticate with email and password, returning an access token and refresh token."
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
                                "detail": "The email or password is incorrect.",
                                "instance": "/api/auth/login",
                                "code": "Auth.InvalidCredentials",
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
