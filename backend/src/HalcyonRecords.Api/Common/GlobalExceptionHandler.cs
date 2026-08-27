using HalcyonRecords.Api.Common.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

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
        logger.UnhandledException(exception);

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
                BadHttpRequestException badHttpRequestException => (
                    badHttpRequestException.StatusCode,
                    ReasonPhrases.GetReasonPhrase(badHttpRequestException.StatusCode),
                    "The request did not meet the server's requirements and could not be processed."
                ),
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
                DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "Conflict",
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
