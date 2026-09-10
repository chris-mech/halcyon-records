using System.Globalization;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace HalcyonRecords.Api.IntegrationTests.Common.HealthChecks;

public class HealthCheckEndpointTests(
    SqlServerContainerFixture sqlFixture,
    MeilisearchContainerFixture meilisearchFixture
)
{
    private const int ManagementPort = 8081;

    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    public async Task Get_PublicPortWithManagementPortConfigured_ReturnsNotFound(string path)
    {
        await using var factory = new ProductionApiWebApplicationFactory(
            sqlFixture,
            meilisearchFixture
        );
        SetManagementPort(ManagementPort);
        try
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            SetManagementPort(null);
        }
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    public async Task Get_NoManagementPortOutsideDevelopment_ReturnsNotFound(string path)
    {
        await using var factory = new ProductionApiWebApplicationFactory(
            sqlFixture,
            meilisearchFixture
        );
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    public async Task Get_ManagementPort_ReturnsHealthy(string path)
    {
        await using var factory = new ProductionApiWebApplicationFactory(
            sqlFixture,
            meilisearchFixture
        );
        SetManagementPort(ManagementPort);
        try
        {
            var httpContext = await factory.Server.SendAsync(
                context =>
                {
                    context.Request.Path = path;
                    context.Connection.LocalPort = ManagementPort;
                },
                TestContext.Current.CancellationToken
            );

            using var reader = new StreamReader(httpContext.Response.Body);
            var body = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            body.Should().Be("Healthy");
        }
        finally
        {
            SetManagementPort(null);
        }
    }

    private static void SetManagementPort(int? port) =>
        Environment.SetEnvironmentVariable(
            "HealthChecks__ManagementPort",
            port?.ToString(CultureInfo.InvariantCulture)
        );
}
