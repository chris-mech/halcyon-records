using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Common.Results;

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
            var (statusCode, title) = firstError.Type.ToProblemDetailsParts();

            return TypedResults.Problem(
                detail: firstError.Description,
                statusCode: statusCode,
                title: title
            );
        }

        public Results<TSuccess1, TSuccess2, ProblemHttpResult, ValidationProblem> Problem<
            TSuccess1,
            TSuccess2
        >()
            where TSuccess1 : IResult
            where TSuccess2 : IResult
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
            var (statusCode, title) = firstError.Type.ToProblemDetailsParts();

            return TypedResults.Problem(
                detail: firstError.Description,
                statusCode: statusCode,
                title: title
            );
        }
    }

    extension(ErrorType errorType)
    {
        private (int StatusCode, string Title) ToProblemDetailsParts() =>
            errorType switch
            {
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
            };
    }
}
