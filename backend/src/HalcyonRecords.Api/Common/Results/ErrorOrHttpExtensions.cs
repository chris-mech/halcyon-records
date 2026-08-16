using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Common.Results;

public static class ErrorOrHttpExtensions
{
    extension(List<Error> errors)
    {
        public Results<TSuccess, ProblemHttpResult> Problem<TSuccess>()
            where TSuccess : IResult
        {
            errors.EnsureNotEmpty();

            if (errors.Any(error => error.Type == ErrorType.Validation))
            {
                throw new InvalidOperationException(
                    "Problem() was called with a validation error, but this endpoint's "
                        + "Results<> union has no ValidationProblem branch. Use "
                        + "ProblemWithValidation() instead if this request can be validated."
                );
            }

            return errors.BuildProblem();
        }

        public Results<
            TSuccess,
            ProblemHttpResult,
            ValidationProblem
        > ProblemWithValidationProblem<TSuccess>()
            where TSuccess : IResult
        {
            errors.EnsureNotEmpty();

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

            return errors.BuildProblem();
        }

        private void EnsureNotEmpty()
        {
            if (errors.Count == 0)
            {
                throw new InvalidOperationException(
                    "Problem()/ProblemWithValidation() was called with an empty error list."
                );
            }
        }

        private ProblemHttpResult BuildProblem()
        {
            var firstError = errors[0];
            var (statusCode, title) = firstError.Type.ToProblemDetailsParts();

            return TypedResults.Problem(
                detail: firstError.Description,
                statusCode: statusCode,
                title: title,
                extensions: new Dictionary<string, object?> { ["code"] = firstError.Code }
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
