using FluentAssertions;
using HalcyonRecords.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HalcyonRecords.Api.UnitTests.Common;

public class GlobalExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(BadHttpRequestException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(TaskCanceledException), StatusCodes.Status408RequestTimeout)]
    [InlineData(typeof(HttpRequestException), StatusCodes.Status502BadGateway)]
    [InlineData(typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(DbUpdateException), StatusCodes.Status500InternalServerError)]
    [InlineData(typeof(RetryLimitExceededException), StatusCodes.Status503ServiceUnavailable)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status500InternalServerError)]
    public async Task TryHandleAsync_KnownExceptionTypes_ProduceTheExpectedStatusCode(
        Type exceptionType,
        int expectedStatusCode
    )
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test")!;
        var httpContext = new DefaultHttpContext();
        var handler = new GlobalExceptionHandler(
            Substitute.For<IProblemDetailsService>(),
            Substitute.For<ILogger<GlobalExceptionHandler>>()
        );

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_RetryLimitExceededException_SetsRetryAfterHeader()
    {
        var exception = new RetryLimitExceededException("test");
        var httpContext = new DefaultHttpContext();
        var handler = new GlobalExceptionHandler(
            Substitute.For<IProblemDetailsService>(),
            Substitute.For<ILogger<GlobalExceptionHandler>>()
        );

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.Headers.RetryAfter.ToString().Should().Be("30");
    }
}
