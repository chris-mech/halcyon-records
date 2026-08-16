using System.Security.Claims;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/auth/me",
                async Task<Results<Ok<CurrentUserResponse>, ProblemHttpResult>> (
                    ClaimsPrincipal claimsPrincipal,
                    ISender sender
                ) =>
                {
                    var publicId = Guid.Parse(
                        claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub)!
                    );

                    ErrorOr<CurrentUserResponse> result = await sender.Send(
                        new GetCurrentUserQuery(publicId)
                    );

                    return result.Match<Results<Ok<CurrentUserResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<CurrentUserResponse>>()
                    );
                }
            )
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            );
    }
}
