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
using Microsoft.Extensions.Options;
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
                    [Description("The page number to return.")] int? page,
                    [Description("The number of items per page.")] int? pageSize,
                    IOptions<OrdersPaginationOptions> paginationOptions,
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var pagination = paginationOptions.Value;

                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<PagedResult<OrderSummaryResponse>> result = await sender.Send(
                        new GetOrdersQuery(
                            publicId,
                            page ?? pagination.MinPage,
                            pageSize ?? pagination.DefaultPageSize
                        )
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
                    var pagination = context
                        .ApplicationServices.GetRequiredService<IOptions<OrdersPaginationOptions>>()
                        .Value;

                    operation.SetParameterExample("page", JsonValue.Create(1));
                    operation.SetParameterMinimum("page", pagination.MinPage);
                    operation.SetParameterDefault("page", JsonValue.Create(pagination.MinPage));
                    operation.SetParameterExample("pageSize", JsonValue.Create(10));
                    operation.SetParameterRange(
                        "pageSize",
                        pagination.MinPageSize,
                        pagination.MaxPageSize
                    );
                    operation.SetParameterDefault(
                        "pageSize",
                        JsonValue.Create(pagination.DefaultPageSize)
                    );
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
                    return Task.CompletedTask;
                }
            );
    }
}
