using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using HalcyonRecords.Api.Infrastructure.Seed;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.BackgroundJobs;

public class JobRunnerTests(
    SqlServerContainerFixture sqlFixture,
    MeilisearchContainerFixture meilisearchFixture
) : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory = new(sqlFixture, meilisearchFixture);

    public async ValueTask InitializeAsync()
    {
        await sqlFixture.ResetAsync();
        await meilisearchFixture.ResetAsync();
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task RunAsync_Migrate_ReturnsSuccess()
    {
        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "migrate",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_Seed_PopulatesCatalogAndShowcaseAccount()
    {
        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "seed",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(0);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (await dbContext.Albums.AnyAsync(TestContext.Current.CancellationToken)).Should().BeTrue();
        (
            await dbContext.Users.AnyAsync(
                u => u.Email == DbSeeder.ShowcaseAccountEmail,
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task RunAsync_Restock_RestoresDepletedAlbumsToRestockLevel()
    {
        AlbumId albumId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var album = new Album
            {
                Title = "Job Runner Restock Album",
                UnitsInStock = 1,
                RestockUnitsInStock = 12,
                PriceInPence = 1000,
            };
            dbContext.Albums.Add(album);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            albumId = album.Id;
        }

        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "restock",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(0);

        using var assertScope = _factory.Services.CreateScope();
        var reloaded = await assertScope
            .ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Albums.AsNoTracking()
            .SingleAsync(a => a.Id == albumId, TestContext.Current.CancellationToken);
        reloaded.UnitsInStock.Should().Be(12);
    }

    [Fact]
    public async Task RunAsync_AccountMaintenance_RemovesStaleAccountAndResetsShowcaseAccount()
    {
        var now = DateTimeOffset.UtcNow;
        int showcaseUserId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var staleUser = new User
            {
                UserName = "stale-job-runner-user@test.invalid",
                Email = "stale-job-runner-user@test.invalid",
                FirstName = "Stale",
                LastName = "User",
                RegisteredAt = now.AddDays(-200),
                LastActiveAt = now.AddDays(-200),
            };
            await userManager.CreateAsync(staleUser, "ValidPassword123!");

            var showcaseUser = new User
            {
                UserName = "showcase-job-runner-user@test.invalid",
                Email = "showcase-job-runner-user@test.invalid",
                FirstName = "Demo",
                LastName = "Shopper",
                RegisteredAt = now.AddYears(-1),
                LastActiveAt = now,
                IsShowcaseAccount = true,
            };
            await userManager.CreateAsync(showcaseUser, "ValidPassword123!");
            showcaseUserId = showcaseUser.Id;

            var albums = new List<Album>
            {
                new()
                {
                    Title = "Account Maintenance Album One",
                    UnitsInStock = 10,
                    RestockUnitsInStock = 10,
                    PriceInPence = 1000,
                },
                new()
                {
                    Title = "Account Maintenance Album Two",
                    UnitsInStock = 10,
                    RestockUnitsInStock = 10,
                    PriceInPence = 1200,
                },
                new()
                {
                    Title = "Account Maintenance Album Three",
                    UnitsInStock = 10,
                    RestockUnitsInStock = 10,
                    PriceInPence = 1500,
                },
            };
            dbContext.Albums.AddRange(albums);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            dbContext.Orders.Add(
                new Order
                {
                    UserId = showcaseUser.Id,
                    OrderNumber = "HR-999999",
                    IdempotencyKey = Guid.NewGuid(),
                    ContactFirstName = showcaseUser.FirstName,
                    ContactLastName = showcaseUser.LastName,
                    ContactEmail = showcaseUser.Email!,
                    TotalInPence = albums[0].PriceInPence,
                    PlacedAt = now.AddHours(-2),
                    OrderItems =
                    [
                        new OrderItem
                        {
                            AlbumId = albums[0].Id,
                            Album = albums[0],
                            Quantity = 1,
                            PriceAtPurchaseInPence = albums[0].PriceInPence,
                        },
                    ],
                }
            );
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "account-maintenance",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(0);

        using var assertScope = _factory.Services.CreateScope();
        var assertDbContext =
            assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (
            await assertDbContext.Users.AnyAsync(
                u => u.Email == "stale-job-runner-user@test.invalid",
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .BeFalse();

        var showcaseOrderNumbers = await assertDbContext
            .Orders.Where(o => o.UserId == showcaseUserId)
            .Select(o => o.OrderNumber)
            .ToListAsync(TestContext.Current.CancellationToken);
        showcaseOrderNumbers.Should().BeEquivalentTo("HR-DEMO-01", "HR-DEMO-02");
    }

    [Fact]
    public async Task RunAsync_Reindex_RebuildsMeilisearchIndex()
    {
        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "reindex",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(0);
        var settings = await meilisearchFixture
            .Client.Index(MeilisearchContainerFixture.IndexName)
            .GetSettingsAsync(TestContext.Current.CancellationToken);
        settings.SearchableAttributes.Should().Contain("title");
    }

    [Fact]
    public async Task RunAsync_UnknownJob_ReturnsFailureExitCode()
    {
        var exitCode = await JobRunner.RunAsync(
            _factory.Services,
            "not-a-real-job",
            TestContext.Current.CancellationToken
        );

        exitCode.Should().Be(1);
    }
}
