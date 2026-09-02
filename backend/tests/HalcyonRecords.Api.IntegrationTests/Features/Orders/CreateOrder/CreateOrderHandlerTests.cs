using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Features.Orders.CreateOrder;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Features.Orders.CreateOrder;

public class CreateOrderHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly IOptions<ShopOptions> s_shopOptions = Options.Create(new ShopOptions());

    private HybridCache Cache { get; } = NewCache();

    private CreateOrderHandler Handler =>
        new(DbContext, s_albumSqids, s_shopOptions, TimeProvider, Cache);

    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    [Fact]
    public async Task Handle_UnknownPublicId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            NewCommand(Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_EmptyCart_ReturnsCartEmptyError()
    {
        var user = await CreateUserAsync("create-order-empty-cart@test.invalid");

        var result = await Handler.Handle(
            NewCommand(user.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.CartEmpty");
    }

    [Fact]
    public async Task Handle_ValidCart_CreatesOrderAndDecrementsStockAndClearsCart()
    {
        var user = await CreateUserAsync("create-order-happy-path@test.invalid");
        var album = NewAlbum("Create Order Happy Path Album", unitsInStock: 5, priceInPence: 1500);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, album, 2);

        var result = await Handler.Handle(
            NewCommand(user.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.TotalInPence.Should().Be(3000);
        result.Value.Items.Should().ContainSingle(i => i.Quantity == 2);

        var updatedAlbum = await DbContext
            .Albums.AsNoTracking()
            .FirstAsync(a => a.Id == album.Id, TestContext.Current.CancellationToken);
        updatedAlbum.UnitsInStock.Should().Be(3);

        var cartItems = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        cartItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InsufficientStock_ReturnsConflictAndRollsBackAllDecrements()
    {
        var user = await CreateUserAsync("create-order-insufficient-stock@test.invalid");
        var inStockAlbum = NewAlbum(
            "Insufficient Stock In Stock Album",
            unitsInStock: 5,
            priceInPence: 1000
        );
        var soldOutAlbum = NewAlbum(
            "Insufficient Stock Sold Out Album",
            unitsInStock: 1,
            priceInPence: 1000
        );
        DbContext.Albums.AddRange(inStockAlbum, soldOutAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, inStockAlbum, 2);
        await AddToCartAsync(user, soldOutAlbum, 2);

        var result = await Handler.Handle(
            NewCommand(user.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.InsufficientStock");

        var reloadedInStockAlbum = await DbContext
            .Albums.AsNoTracking()
            .FirstAsync(a => a.Id == inStockAlbum.Id, TestContext.Current.CancellationToken);
        reloadedInStockAlbum.UnitsInStock.Should().Be(5);

        var orders = await DbContext
            .Orders.Where(o => o.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCart_PersistsContactDetails()
    {
        var user = await CreateUserAsync("create-order-contact-details@test.invalid");
        var album = NewAlbum("Contact Details Album", unitsInStock: 5, priceInPence: 1000);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, album, 1);

        var result = await Handler.Handle(
            new CreateOrderCommand(
                user.PublicId,
                "Gift",
                "Recipient",
                "gift-recipient@test.invalid",
                Guid.NewGuid()
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();

        var order = await DbContext
            .Orders.AsNoTracking()
            .FirstAsync(
                o => o.OrderNumber == result.Value.OrderNumber,
                TestContext.Current.CancellationToken
            );
        order.ContactFirstName.Should().Be("Gift");
        order.ContactLastName.Should().Be("Recipient");
        order.ContactEmail.Should().Be("gift-recipient@test.invalid");
    }

    [Fact]
    public async Task Handle_SameIdempotencyKeyRetried_ReturnsSameOrderWithoutCreatingDuplicate()
    {
        var user = await CreateUserAsync("create-order-idempotent-retry@test.invalid");
        var album = NewAlbum("Idempotent Retry Album", unitsInStock: 5, priceInPence: 1000);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var idempotencyKey = Guid.NewGuid();
        await AddToCartAsync(user, album, 1);

        var first = await Handler.Handle(
            NewCommand(user.PublicId, idempotencyKey),
            TestContext.Current.CancellationToken
        );

        var second = await Handler.Handle(
            NewCommand(user.PublicId, idempotencyKey),
            TestContext.Current.CancellationToken
        );

        first.IsError.Should().BeFalse();
        second.IsError.Should().BeFalse();
        second.Value.OrderNumber.Should().Be(first.Value.OrderNumber);

        var orderCount = await DbContext.Orders.CountAsync(
            o => o.IdempotencyKey == idempotencyKey,
            TestContext.Current.CancellationToken
        );
        orderCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConcurrentRequestsWithSameIdempotencyKey_OnlyOneOrderIsCreated()
    {
        var user = await CreateUserAsync("concurrent-idempotency-key@test.invalid");
        var album = NewAlbum(
            "Concurrent Idempotency Key Album",
            unitsInStock: 10,
            priceInPence: 1000
        );
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, album, 1);

        var idempotencyKey = Guid.NewGuid();
        await using var dbContext2 = NewDbContext();
        var handler1 = Handler;
        var handler2 = new CreateOrderHandler(
            dbContext2,
            s_albumSqids,
            s_shopOptions,
            TimeProvider,
            Cache
        );

        var task1 = handler1.Handle(
            NewCommand(user.PublicId, idempotencyKey),
            TestContext.Current.CancellationToken
        );
        var task2 = handler2.Handle(
            NewCommand(user.PublicId, idempotencyKey),
            TestContext.Current.CancellationToken
        );
        var results = await Task.WhenAll(task1, task2);

        results.Should().OnlyContain(r => !r.IsError);
        results[0].Value.OrderNumber.Should().Be(results[1].Value.OrderNumber);

        var orderCount = await DbContext.Orders.CountAsync(
            o => o.IdempotencyKey == idempotencyKey,
            TestContext.Current.CancellationToken
        );
        orderCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConcurrentRequestsForLastUnit_OnlyOneSucceedsAndStockNeverGoesNegative()
    {
        var buyerOne = await CreateUserAsync("concurrent-stock-buyer-one@test.invalid");
        var buyerTwo = await CreateUserAsync("concurrent-stock-buyer-two@test.invalid");
        var album = NewAlbum("Concurrent Stock Race Album", unitsInStock: 1, priceInPence: 1000);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(buyerOne, album, 1);
        await AddToCartAsync(buyerTwo, album, 1);

        await using var dbContext2 = NewDbContext();
        var handler1 = Handler;
        var handler2 = new CreateOrderHandler(
            dbContext2,
            s_albumSqids,
            s_shopOptions,
            TimeProvider,
            Cache
        );

        var task1 = handler1.Handle(
            NewCommand(buyerOne.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        var task2 = handler2.Handle(
            NewCommand(buyerTwo.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        var results = await Task.WhenAll(task1, task2);

        results.Count(r => !r.IsError).Should().Be(1);
        results
            .Count(r => r.IsError && r.FirstError.Code == "Order.InsufficientStock")
            .Should()
            .Be(1);

        var finalAlbum = await DbContext
            .Albums.AsNoTracking()
            .FirstAsync(a => a.Id == album.Id, TestContext.Current.CancellationToken);
        finalAlbum.UnitsInStock.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SuccessfulOrder_EvictsAlbumAndAlbumListCacheTags()
    {
        var user = await CreateUserAsync("create-order-cache-eviction@test.invalid");
        var album = NewAlbum("Cache Eviction Album", unitsInStock: 5, priceInPence: 1000);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, album, 1);

        var albumTag = $"album:{s_albumSqids.Encode(album.Id.Value)}";

        await Cache.GetOrCreateAsync(
            "cache-eviction-album-key",
            async _ => "stale-album-value",
            tags: [albumTag],
            cancellationToken: TestContext.Current.CancellationToken
        );
        await Cache.GetOrCreateAsync(
            "cache-eviction-list-key",
            async _ => "stale-list-value",
            tags: ["albums"],
            cancellationToken: TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            NewCommand(user.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );
        result.IsError.Should().BeFalse();

        var albumCacheValueAfter = await Cache.GetOrCreateAsync(
            "cache-eviction-album-key",
            async _ => "fresh-album-value",
            tags: [albumTag],
            cancellationToken: TestContext.Current.CancellationToken
        );
        var listCacheValueAfter = await Cache.GetOrCreateAsync(
            "cache-eviction-list-key",
            async _ => "fresh-list-value",
            tags: ["albums"],
            cancellationToken: TestContext.Current.CancellationToken
        );

        albumCacheValueAfter.Should().Be("fresh-album-value");
        listCacheValueAfter.Should().Be("fresh-list-value");
    }

    [Fact]
    public async Task Handle_UserDeletedBetweenLookupAndSave_ReturnsUserNotFoundError()
    {
        var user = await CreateUserAsync("create-order-user-deleted-mid-checkout@test.invalid");
        var album = NewAlbum(
            "User Deleted Mid Checkout Album",
            unitsInStock: 5,
            priceInPence: 1000
        );
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddToCartAsync(user, album, 1);

        var deletingTimeProvider = new UserDeletingTimeProvider(NewDbContext, user.Id);
        var handler = new CreateOrderHandler(
            DbContext,
            s_albumSqids,
            s_shopOptions,
            deletingTimeProvider,
            Cache
        );

        var result = await handler.Handle(
            NewCommand(user.PublicId, Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.UserNotFound");

        var finalAlbum = await DbContext
            .Albums.AsNoTracking()
            .FirstAsync(a => a.Id == album.Id, TestContext.Current.CancellationToken);
        finalAlbum.UnitsInStock.Should().Be(5);
    }

    private ApplicationDbContext NewDbContext() =>
        new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    fixture.ConnectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                )
                .Options
        );

    private static CreateOrderCommand NewCommand(Guid publicId, Guid idempotencyKey) =>
        new(publicId, "Order", "Contact", "order-contact@test.invalid", idempotencyKey);

    private async Task AddToCartAsync(User user, Album album, int quantity)
    {
        var cart = await DbContext
            .Carts.Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id, TestContext.Current.CancellationToken);
        if (cart is null)
        {
            cart = new Cart { UserId = user.Id };
            DbContext.Carts.Add(cart);
        }

        cart.CartItems.Add(new CartItem { AlbumId = album.Id, Quantity = quantity });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<User> CreateUserAsync(string email)
    {
        await RegisterHandler.Handle(
            new RegisterCommand("Create Order", "Test User", email, "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        return (await UserManager.FindByEmailAsync(email))!;
    }

    private static HybridCache NewCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    private static Album NewAlbum(string title, int unitsInStock, int priceInPence) =>
        new()
        {
            Title = title,
            UnitsInStock = unitsInStock,
            PriceInPence = priceInPence,
        };

    private sealed class UserDeletingTimeProvider(
        Func<ApplicationDbContext> newDbContext,
        int userId
    ) : TimeProvider
    {
        private bool _deleted;

        public override DateTimeOffset GetUtcNow()
        {
            if (!_deleted)
            {
                _deleted = true;
                using var dbContext = newDbContext();
                dbContext
                    .Users.Where(u => u.Id == userId)
                    .ExecuteDeleteAsync()
                    .GetAwaiter()
                    .GetResult();
            }

            return DateTimeOffset.UtcNow;
        }
    }
}
