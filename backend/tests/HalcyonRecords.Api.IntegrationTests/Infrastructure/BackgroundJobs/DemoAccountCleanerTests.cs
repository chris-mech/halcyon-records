using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.BackgroundJobs;

public class DemoAccountCleanerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly DateTimeOffset s_now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private DemoAccountCleaner Cleaner =>
        new(
            DbContext,
            TimeProvider,
            Options.Create(
                new AccountMaintenanceOptions
                {
                    InactivityThresholdDays = 7,
                    MaxAccountAgeDays = 90,
                }
            ),
            NullLogger<DemoAccountCleaner>.Instance
        );

    [Fact]
    public async Task RemoveStaleAccountsAsync_AccountInactiveBeyondThreshold_IsRemoved()
    {
        TimeProvider.SetUtcNow(s_now);
        var staleUser = await CreateUserAsync(
            "stale-user@test.invalid",
            registeredAt: s_now.AddDays(-60),
            lastActiveAt: s_now.AddDays(-8)
        );

        await Cleaner.RemoveStaleAccountsAsync(TestContext.Current.CancellationToken);

        (await FindUserAsync(staleUser.Email!)).Should().BeNull();
    }

    [Fact]
    public async Task RemoveStaleAccountsAsync_AccountActiveAndWithinMaxAge_Survives()
    {
        TimeProvider.SetUtcNow(s_now);
        var activeUser = await CreateUserAsync(
            "active-user@test.invalid",
            registeredAt: s_now.AddDays(-30),
            lastActiveAt: s_now.AddDays(-1)
        );

        await Cleaner.RemoveStaleAccountsAsync(TestContext.Current.CancellationToken);

        (await FindUserAsync(activeUser.Email!)).Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveStaleAccountsAsync_AccountKeptActiveButPastMaxAge_IsRemoved()
    {
        TimeProvider.SetUtcNow(s_now);
        var longLivedUser = await CreateUserAsync(
            "long-lived-user@test.invalid",
            registeredAt: s_now.AddDays(-91),
            lastActiveAt: s_now.AddDays(-1)
        );

        await Cleaner.RemoveStaleAccountsAsync(TestContext.Current.CancellationToken);

        (await FindUserAsync(longLivedUser.Email!)).Should().BeNull();
    }

    [Fact]
    public async Task RemoveStaleAccountsAsync_ShowcaseAccountPastBothThresholds_Survives()
    {
        TimeProvider.SetUtcNow(s_now);
        var demoUser = await CreateUserAsync(
            "permanent-user@test.invalid",
            registeredAt: s_now.AddYears(-1),
            lastActiveAt: s_now.AddYears(-1),
            isShowcaseAccount: true
        );

        await Cleaner.RemoveStaleAccountsAsync(TestContext.Current.CancellationToken);

        (await FindUserAsync(demoUser.Email!)).Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveStaleAccountsAsync_StaleAccountWithOrder_CascadesDeletion()
    {
        TimeProvider.SetUtcNow(s_now);
        var staleUser = await CreateUserAsync(
            "stale-with-data@test.invalid",
            registeredAt: s_now.AddDays(-60),
            lastActiveAt: s_now.AddDays(-8)
        );

        var album = new Album
        {
            Title = "Cascade Test Album",
            UnitsInStock = 10,
            RestockUnitsInStock = 10,
            PriceInPence = 1000,
        };
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbContext.Orders.Add(
            new Order
            {
                UserId = staleUser.Id,
                OrderNumber = "HR-000001",
                IdempotencyKey = Guid.NewGuid(),
                ContactFirstName = staleUser.FirstName,
                ContactLastName = staleUser.LastName,
                ContactEmail = staleUser.Email!,
                TotalInPence = 1000,
                PlacedAt = s_now.AddDays(-8),
                OrderItems =
                [
                    new OrderItem
                    {
                        AlbumId = album.Id,
                        Album = album,
                        Quantity = 1,
                        PriceAtPurchaseInPence = 1000,
                    },
                ],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Cleaner.RemoveStaleAccountsAsync(TestContext.Current.CancellationToken);

        (
            await DbContext.Orders.AnyAsync(
                o => o.UserId == staleUser.Id,
                TestContext.Current.CancellationToken
            )
        )
            .Should()
            .BeFalse();
    }

    private async Task<User> CreateUserAsync(
        string email,
        DateTimeOffset registeredAt,
        DateTimeOffset lastActiveAt,
        bool isShowcaseAccount = false
    )
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            RegisteredAt = registeredAt,
            LastActiveAt = lastActiveAt,
            IsShowcaseAccount = isShowcaseAccount,
        };
        await UserManager.CreateAsync(user, "ValidPassword123!");
        return user;
    }

    private Task<User?> FindUserAsync(string email) =>
        DbContext.Users.SingleOrDefaultAsync(
            u => u.Email == email,
            TestContext.Current.CancellationToken
        );
}
