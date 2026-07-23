using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HalcyonRecords.Api.Common;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "An unhandled exception occurred while processing the request.");

        var (statusCode, title, detail) = exception.ToProblemDetailsParts();

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
        };

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception,
            }
        );
    }
}

file static class ExceptionMappingExtensions
{
    extension(Exception exception)
    {
        public (int StatusCode, string Title, string Detail) ToProblemDetailsParts() =>
            exception switch
            {
                TaskCanceledException => (
                    StatusCodes.Status408RequestTimeout,
                    "Request Timeout",
                    "Please try again later."
                ),
                HttpRequestException => (
                    StatusCodes.Status502BadGateway,
                    "Bad Gateway",
                    "Please try again later."
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "Please try again later."
                ),
            };
    }
}
