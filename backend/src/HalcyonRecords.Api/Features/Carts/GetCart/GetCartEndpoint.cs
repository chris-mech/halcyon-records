using System.Security.Claims;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Carts.GetCart;

public sealed class GetCartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/cart",
                async Task<Results<Ok<IReadOnlyList<CartItemResponse>>, ProblemHttpResult>> (
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<IReadOnlyList<CartItemResponse>> result = await sender.Send(
                        new GetCartQuery(publicId)
                    );

                    return result.Match<
                        Results<Ok<IReadOnlyList<CartItemResponse>>, ProblemHttpResult>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<CartItemResponse>>>()
                    );
                }
            )
            .WithName("GetCart")
            .WithTags("Carts")
            .WithSummary("Get the current user's cart contents.")
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            );
    }
}
