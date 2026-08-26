using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders",
                async Task<
                    Results<
                        Ok<PagedResult<OrderSummaryResponse>>,
                        ProblemHttpResult,
                        ValidationProblem
                    >
                > (
                    [Description("The page number to return, starting at 1. Defaults to 1.")]
                        int? page,
                    [Description("The number of items per page, from 1 to 25. Defaults to 10.")]
                        int? pageSize,
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<PagedResult<OrderSummaryResponse>> result = await sender.Send(
                        new GetOrdersQuery(publicId, page ?? 1, pageSize ?? 10)
                    );

                    return result.Match<
                        Results<
                            Ok<PagedResult<OrderSummaryResponse>>,
                            ProblemHttpResult,
                            ValidationProblem
                        >
                    >(
                        response => TypedResults.Ok(response),
                        errors =>
                            errors.ProblemWithValidationProblem<
                                Ok<PagedResult<OrderSummaryResponse>>
                            >()
                    );
                }
            )
            .WithName("GetOrders")
            .WithTags("Orders")
            .WithSummary("List the current user's orders, paginated.")
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("page", JsonValue.Create(1));
                    operation.SetParameterExample("pageSize", JsonValue.Create(10));
                    return Task.CompletedTask;
                }
            );
    }
}
