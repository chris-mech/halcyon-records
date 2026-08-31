using System.Net;
using FluentAssertions;

namespace HalcyonRecords.Api.IntegrationTests.Common.RateLimiting;

public class RateLimitingTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory = new(fixture);

    public ValueTask InitializeAsync() => new(fixture.ResetAsync());

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task Get_ForwardedForHeader_PartitionsIndependentlyPerClientIp()
    {
        Environment.SetEnvironmentVariable("RateLimiting__PermitLimit", "2");
        try
        {
            using var client = _factory.CreateClient();

            var firstClientResponses = new List<HttpStatusCode>();
            for (var i = 0; i < 3; i++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/api/genres");
                request.Headers.Add("X-Forwarded-For", "203.0.113.10");
                var response = await client.SendAsync(
                    request,
                    TestContext.Current.CancellationToken
                );
                firstClientResponses.Add(response.StatusCode);
            }

            firstClientResponses
                .Should()
                .Equal(HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests);

            using var secondClientRequest = new HttpRequestMessage(HttpMethod.Get, "/api/genres");
            secondClientRequest.Headers.Add("X-Forwarded-For", "203.0.113.20");
            var secondClientResponse = await client.SendAsync(
                secondClientRequest,
                TestContext.Current.CancellationToken
            );

            secondClientResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("RateLimiting__PermitLimit", null);
        }
    }
}
