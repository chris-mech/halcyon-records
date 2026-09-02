using HalcyonRecords.Api.Infrastructure.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(
    typeof(HalcyonRecords.Api.IntegrationTests.Common.SqlServerContainerFixture)
)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace HalcyonRecords.Api.IntegrationTests.Common;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2025-latest"
    ).Build();

    private SqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using (var dbContext = new ApplicationDbContext(options))
        {
            await dbContext.Database.MigrateAsync();
        }

        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions { TablesToIgnore = ["__EFMigrationsHistory"] }
        );
    }

    public Task ResetAsync() => _respawner.ResetAsync(_connection);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
