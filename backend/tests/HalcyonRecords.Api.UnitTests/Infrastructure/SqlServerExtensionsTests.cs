using FluentAssertions;
using HalcyonRecords.Api.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HalcyonRecords.Api.UnitTests.Infrastructure;

public class SqlServerExtensionsTests
{
    [Fact]
    public void AddApiSqlServer_ShouldConfigureRetryPolicy()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["ConnectionStrings:halcyonrecords"] = "Server=fake;Database=fake";

        builder.AddApiSqlServer();

        using var host = builder.Build();
        var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();
        var strategy = (ExecutionStrategy)dbContext.Database.CreateExecutionStrategy();

        strategy.MaxRetryCount.Should().Be(10);
        strategy.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(15));
    }
}
