using HalcyonRecords.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Common;

public abstract class IntegrationTestBase(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    protected ApplicationDbContext DbContext { get; } =
        new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    fixture.ConnectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                )
                .Options
        );

    public virtual async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => DbContext.DisposeAsync();
}
