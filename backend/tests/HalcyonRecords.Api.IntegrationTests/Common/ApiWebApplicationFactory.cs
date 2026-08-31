using HalcyonRecords.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HalcyonRecords.Api.IntegrationTests.Common;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string JwtSigningKey = "integration-test-signing-key-at-least-32-bytes-long";
    public const string JwtIssuer = "HalcyonRecords.Api.Tests";
    public const string JwtAudience = "HalcyonRecords.Api.Tests";

    public ApiWebApplicationFactory(
        SqlServerContainerFixture fixture,
        MeilisearchContainerFixture? meilisearchFixture = null
    )
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__halcyonrecords",
            fixture.ConnectionString
        );
        Environment.SetEnvironmentVariable($"{JwtOptions.SectionName}__SigningKey", JwtSigningKey);
        Environment.SetEnvironmentVariable($"{JwtOptions.SectionName}__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable($"{JwtOptions.SectionName}__Audience", JwtAudience);

        if (meilisearchFixture is not null)
        {
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__meilisearch",
                meilisearchFixture.ConnectionString
            );
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Testing");
}
