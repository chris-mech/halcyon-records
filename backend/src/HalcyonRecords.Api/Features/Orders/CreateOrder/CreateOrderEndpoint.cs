using System.Security.Claims;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
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
            );
    }
}
