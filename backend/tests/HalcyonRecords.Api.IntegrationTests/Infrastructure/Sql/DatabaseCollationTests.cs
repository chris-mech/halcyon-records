using FluentAssertions;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.Sql;

public class DatabaseCollationTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Database_UsesConfiguredCollation()
    {
        var connection = (SqlConnection)DbContext.Database.GetDbConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT DATABASEPROPERTYEX(DB_NAME(), 'Collation')";

        var collation = (string?)
            await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        collation.Should().Be("Latin1_General_100_CI_AS_SC");
    }
}
