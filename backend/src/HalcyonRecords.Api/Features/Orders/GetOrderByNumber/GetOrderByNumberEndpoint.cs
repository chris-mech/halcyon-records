using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed class GetOrderByNumberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{orderNumber}",
                async Task<Results<Ok<OrderDetailResponse>, ProblemHttpResult>> (
                    [Description("The order number, as returned when the order was placed.")]
                        string orderNumber,
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<OrderDetailResponse> result = await sender.Send(
                        new GetOrderByNumberQuery(publicId, orderNumber)
                    );

                    return result.Match<Results<Ok<OrderDetailResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<OrderDetailResponse>>()
                    );
                }
            )
            .WithName("GetOrderByNumber")
            .WithTags("Orders")
            .WithSummary("Get a single order's detail by order number.")
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("orderNumber", JsonValue.Create("HR-284917"));
                    operation.SetResponseExample(
                        "404",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                "title": "Not Found",
                                "status": 404,
                                "detail": "Order 'HR-284917' not found.",
                                "instance": "/api/orders/HR-284917",
                                "code": "Order.NotFound",
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
