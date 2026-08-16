using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/register",
                async Task<Results<Ok, ProblemHttpResult, ValidationProblem>> (
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

                    return result.Match<Results<Ok, ProblemHttpResult, ValidationProblem>>(
                        _ => TypedResults.Ok(),
                        errors => errors.ProblemWithValidationProblem<Ok>()
                    );
                }
            )
            .WithName("Register");
    }
}
