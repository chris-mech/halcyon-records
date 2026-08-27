using System.Security.Claims;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed class SyncCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/cart/sync",
                async Task<Results<NoContent, ProblemHttpResult, ValidationProblem>> (
                    SyncCartRequest request,
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    var items = request
                        .Items.Select(i => new SyncCartItem(i.AlbumSqid, i.Quantity))
                        .ToList();

                    ErrorOr<Success> result = await sender.Send(
                        new SyncCartCommand(publicId, items)
                    );

                    return result.Match<Results<NoContent, ProblemHttpResult, ValidationProblem>>(
                        _ => TypedResults.NoContent(),
                        errors => errors.ProblemWithValidationProblem<NoContent>()
                    );
                }
            )
            .WithName("SyncCart")
            .WithTags("Carts")
            .WithSummary("Replace the current user's cart contents with the given items.")
            .WithDescription(
                "Items with an unrecognized album sqid are silently ignored rather than causing "
                    + "an error. Duplicate entries for the same album are summed into a single "
                    + "quantity."
            )
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetResponseExample(
                        "404",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                "title": "Not Found",
                                "status": 404,
                                "detail": "The authenticated user no longer exists.",
                                "instance": "/api/cart/sync",
                                "code": "Auth.UserNotFound",
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
