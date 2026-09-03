using HalcyonRecords.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HalcyonRecords.Api.IntegrationTests.Common;

internal sealed class ProductionApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ProductionApiWebApplicationFactory(SqlServerContainerFixture fixture)
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__halcyonrecords",
            fixture.ConnectionString
        );
        Environment.SetEnvironmentVariable(
            $"{JwtOptions.SectionName}__SigningKey",
            ApiWebApplicationFactory.JwtSigningKey
        );
        Environment.SetEnvironmentVariable(
            $"{JwtOptions.SectionName}__Issuer",
            ApiWebApplicationFactory.JwtIssuer
        );
        Environment.SetEnvironmentVariable(
            $"{JwtOptions.SectionName}__Audience",
            ApiWebApplicationFactory.JwtAudience
        );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Production");
}
