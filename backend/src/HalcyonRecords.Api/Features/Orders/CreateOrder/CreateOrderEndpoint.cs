using System.Security.Claims;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/orders",
                async Task<
                    Results<Created<CreateOrderResponse>, ProblemHttpResult, ValidationProblem>
                > (CreateOrderRequest request, ClaimsPrincipal claimsPrincipal, ISender sender) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<CreateOrderResponse> result = await sender.Send(
                        new CreateOrderCommand(
                            publicId,
                            request.ContactFirstName,
                            request.ContactLastName,
                            request.ContactEmail,
                            request.IdempotencyKey
                        )
                    );

                    return result.Match<
                        Results<Created<CreateOrderResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response =>
                            TypedResults.Created($"/orders/{response.OrderNumber}", response),
                        errors =>
                            errors.ProblemWithValidationProblem<Created<CreateOrderResponse>>()
                    );
                }
            )
            .WithName("CreateOrder")
            .WithTags("Orders")
            .WithSummary("Place an order from the current user's cart.")
            .WithDescription(
                "Places the order in a single transaction: stock is decremented per item, the "
                    + "cart is cleared, and a sequential order number is assigned. Retrying with "
                    + "the same idempotency key returns the original order instead of creating a "
                    + "duplicate."
            )
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status409Conflict,
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
                                "instance": "/api/orders",
                                "code": "Auth.UserNotFound",
                                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00"
                            }
                            """
                        )!
                    );
                    operation.SetResponseExample(
                        "409",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                                "title": "Conflict",
                                "status": 409,
                                "detail": "Sorry, 'Midnight Static' just sold out.",
                                "instance": "/api/orders",
                                "code": "Order.InsufficientStock",
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
