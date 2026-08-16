using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/auth/login",
                async Task<Results<Ok<LoginResponse>, ProblemHttpResult, ValidationProblem>> (
                    LoginRequest request,
                    ISender sender
                ) =>
                {
                    ErrorOr<LoginResponse> result = await sender.Send(
                        new LoginCommand(request.Email, request.Password)
                    );

                    return result.Match<
                        Results<Ok<LoginResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<LoginResponse>>()
                    );
                }
            )
            .WithName("Login");
    }
}
