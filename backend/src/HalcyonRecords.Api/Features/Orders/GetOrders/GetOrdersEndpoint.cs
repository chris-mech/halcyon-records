using System.Security.Claims;
using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Endpoints;
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
                > (int? page, int? pageSize, ClaimsPrincipal claimsPrincipal, ISender sender) =>
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
            .RequireAuthorization();
    }
}
