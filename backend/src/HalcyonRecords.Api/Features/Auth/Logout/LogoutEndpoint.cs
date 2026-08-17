using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Auth.Logout;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/logout",
                async Task<Results<NoContent, ProblemHttpResult, ValidationProblem>> (
                    LogoutRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<Success> result = await sender.Send(
                        new LogoutCommand(request.RefreshToken)
                    );

                    return result.Match<Results<NoContent, ProblemHttpResult, ValidationProblem>>(
                        _ => TypedResults.NoContent(),
                        errors => errors.ProblemWithValidationProblem<NoContent>()
                    );
                }
            )
            .WithName("Logout");
    }
}
