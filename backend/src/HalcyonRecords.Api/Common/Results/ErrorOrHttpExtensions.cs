using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Common.Results
{
    public static class ErrorOrHttpExtensions
    {
        extension(List<Error> errors)
        {
            public Results<TSuccess, ProblemHttpResult, ValidationProblem> Problem<TSuccess>()
                where TSuccess : IResult
            {
                if (errors.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Problem() was called with an empty error list."
                    );
                }

                if (errors.All(error => error.Type == ErrorType.Validation))
                {
                    var validationErrors = errors
                        .GroupBy(error => error.Code)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.Description).ToArray()
                        );

                    return TypedResults.ValidationProblem(validationErrors);
                }

                var firstError = errors[0];

                return TypedResults.Problem(
                    statusCode: firstError.Type.ToStatusCode(),
                    title: firstError.Description
                );
            }
        }

        extension(ErrorType errorType)
        {
            private int ToStatusCode() =>
                errorType switch
                {
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError,
                };
        }
    }
}
