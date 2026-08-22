using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.BackgroundJobs;

public class ShowcaseAccountResetterTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly DateTimeOffset s_now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private ShowcaseAccountResetter Resetter =>
        new(
            DbContext,
            Options.Create(new ShopOptions { OrderNumberPrefix = "HR", OrderNumberPadding = 6 }),
            TimeProvider,
            NullLogger<ShowcaseAccountResetter>.Instance
        );

    [Fact]
    public async Task ResetShowcaseAccountAsync_OldExtraOrder_IsRemoved()
    {
        TimeProvider.SetUtcNow(s_now);
        var showcaseUser = await CreateShowcaseUserAsync();
        var albums = await SeedAlbumsAsync();

        await AddOrderAsync(showcaseUser, "HR-999999", albums[0], s_now.AddHours(-2));

        await Resetter.ResetShowcaseAccountAsync(TestContext.Current.CancellationToken);

        var orders = await DbContext
            .Orders.Where(o => o.UserId == showcaseUser.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        orders.Select(o => o.OrderNumber).Should().BeEquivalentTo("HR-DEMO-01", "HR-DEMO-02");
    }

    [Fact]
    public async Task ResetShowcaseAccountAsync_OrderPlacedWithinTheLastHour_Survives()
    {
        TimeProvider.SetUtcNow(s_now);
        var showcaseUser = await CreateShowcaseUserAsync();
        var albums = await SeedAlbumsAsync();

        await AddOrderAsync(showcaseUser, "HR-999998", albums[0], s_now.AddMinutes(-10));

        await Resetter.ResetShowcaseAccountAsync(TestContext.Current.CancellationToken);

        var orders = await DbContext
            .Orders.Where(o => o.UserId == showcaseUser.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        orders
            .Select(o => o.OrderNumber)
            .Should()
            .BeEquivalentTo("HR-999998", "HR-DEMO-01", "HR-DEMO-02");
    }

    [Fact]
    public async Task ResetShowcaseAccountAsync_CartItems_AreAlwaysCleared()
    {
        TimeProvider.SetUtcNow(s_now);
        var showcaseUser = await CreateShowcaseUserAsync();
        var albums = await SeedAlbumsAsync();

        var cart = new Cart { UserId = showcaseUser.Id };
        cart.CartItems.Add(new CartItem { Album = albums[0], Quantity = 1 });
        DbContext.Carts.Add(cart);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Resetter.ResetShowcaseAccountAsync(TestContext.Current.CancellationToken);

        var remainingItems = await DbContext
            .CartItems.Where(ci => ci.CartId == cart.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remainingItems.Should().BeEmpty();
    }

    private async Task<User> CreateShowcaseUserAsync()
    {
        var showcaseUser = new User
        {
            UserName = "demo@test.invalid",
            Email = "demo@test.invalid",
            FirstName = "Demo",
            LastName = "Shopper",
            RegisteredAt = s_now.AddYears(-1),
            LastActiveAt = s_now,
            IsShowcaseAccount = true,
        };
        await UserManager.CreateAsync(showcaseUser, "ValidPassword123!");
        return showcaseUser;
    }

    private async Task<List<Album>> SeedAlbumsAsync()
    {
        var albums = new List<Album>
        {
            new()
            {
                Title = "Reset Test Album One",
                UnitsInStock = 10,
                RestockUnitsInStock = 10,
                PriceInPence = 1000,
            },
            new()
            {
                Title = "Reset Test Album Two",
                UnitsInStock = 10,
                RestockUnitsInStock = 10,
                PriceInPence = 1200,
            },
            new()
            {
                Title = "Reset Test Album Three",
                UnitsInStock = 10,
                RestockUnitsInStock = 10,
                PriceInPence = 1500,
            },
        };
        DbContext.Albums.AddRange(albums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return albums;
    }

    private async Task AddOrderAsync(
        User showcaseUser,
        string orderNumber,
        Album album,
        DateTimeOffset placedAt
    )
    {
        DbContext.Orders.Add(
            new Order
            {
                UserId = showcaseUser.Id,
                OrderNumber = orderNumber,
                IdempotencyKey = Guid.NewGuid(),
                ContactFirstName = showcaseUser.FirstName,
                ContactLastName = showcaseUser.LastName,
                ContactEmail = showcaseUser.Email!,
                TotalInPence = album.PriceInPence,
                PlacedAt = placedAt,
                OrderItems =
                [
                    new OrderItem
                    {
                        AlbumId = album.Id,
                        Album = album,
                        Quantity = 1,
                        PriceAtPurchaseInPence = album.PriceInPence,
                    },
                ],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
