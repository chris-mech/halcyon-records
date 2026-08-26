using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Created = Microsoft.AspNetCore.Http.HttpResults.Created;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/register",
                async Task<Results<Created, ProblemHttpResult, ValidationProblem>> (
                    RegisterRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<Success> result = await sender.Send(
                        new RegisterCommand(
                            request.FirstName,
                            request.LastName,
                            request.Email,
                            request.Password
                        )
                    );

                    return result.Match<Results<Created, ProblemHttpResult, ValidationProblem>>(
                        _ => TypedResults.Created(),
                        errors => errors.ProblemWithValidationProblem<Created>()
                    );
                }
            )
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Create a new user account.")
            .WithDescription(
                "Does not log the new user in or return any tokens; call the login endpoint "
                    + "afterwards to authenticate."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status409Conflict,
                "application/problem+json"
            );
    }
}
