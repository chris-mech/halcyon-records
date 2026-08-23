using System.Security.Claims;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
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
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            );
    }
}
