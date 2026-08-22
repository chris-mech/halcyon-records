using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.BackgroundJobs;

public class AlbumRestockerTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private AlbumRestocker Restocker =>
        new(DbContext, NewCache(), NullLogger<AlbumRestocker>.Instance);

    [Fact]
    public async Task RestockAsync_DepletedAlbum_RestoresRestockLevel()
    {
        var album = new Album
        {
            Title = "Restock Test Album",
            UnitsInStock = 1,
            RestockUnitsInStock = 12,
            PriceInPence = 1000,
        };
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var restocked = await Restocker.RestockAsync(TestContext.Current.CancellationToken);

        restocked.Should().Be(1);
        var reloaded = await DbContext
            .Albums.AsNoTracking()
            .SingleAsync(a => a.Id == album.Id, TestContext.Current.CancellationToken);
        reloaded.UnitsInStock.Should().Be(12);
    }

    [Fact]
    public async Task RestockAsync_AlbumAlreadyAtRestockLevel_LeavesItUntouched()
    {
        var album = new Album
        {
            Title = "Already Stocked Album",
            UnitsInStock = 8,
            RestockUnitsInStock = 8,
            PriceInPence = 1000,
        };
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var restocked = await Restocker.RestockAsync(TestContext.Current.CancellationToken);

        restocked.Should().Be(0);
    }

    private static HybridCache NewCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }
}
