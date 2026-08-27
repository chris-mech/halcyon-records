using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Created = Microsoft.AspNetCore.Http.HttpResults.Created;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/register",
                async Task<Results<Created, ProblemHttpResult, ValidationProblem>> (
                    RegisterRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<Success> result = await sender.Send(
                        new RegisterCommand(
                            request.FirstName,
                            request.LastName,
                            request.Email,
                            request.Password
                        )
                    );

                    return result.Match<Results<Created, ProblemHttpResult, ValidationProblem>>(
                        _ => TypedResults.Created(),
                        errors => errors.ProblemWithValidationProblem<Created>()
                    );
                }
            )
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Create a new user account.")
            .WithDescription(
                "Does not log the new user in or return any tokens; call the login endpoint "
                    + "afterwards to authenticate."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status409Conflict,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetResponseExample(
                        "409",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                                "title": "Conflict",
                                "status": 409,
                                "detail": "An account with email 'john.doe@example.com' already exists.",
                                "instance": "/api/auth/register",
                                "code": "Auth.EmailAlreadyRegistered",
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
