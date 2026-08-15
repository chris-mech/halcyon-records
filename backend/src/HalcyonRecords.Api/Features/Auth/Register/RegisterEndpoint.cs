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
                async Task<Results<Ok<RegisterResponse>, ProblemHttpResult, ValidationProblem>> (
                    RegisterRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<RegisterResponse> result = await sender.Send(
                        new RegisterCommand(
                            request.FirstName,
                            request.LastName,
                            request.Email,
                            request.Password
                        )
                    );

                    return result.Match<
                        Results<Ok<RegisterResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<RegisterResponse>>()
                    );
                }
            )
            .WithName("Register");
    }
}
