using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Features.Carts.SyncCart;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Features.Carts.SyncCart;

public class SyncCartHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();

    private SyncCartHandler Handler => new(DbContext, s_albumSqids);

    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    [Fact]
    public async Task Handle_UnknownPublicId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            new SyncCartCommand(Guid.NewGuid(), []),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_NoExistingCart_CreatesCartWithSyncedItems()
    {
        var user = await CreateUserAsync("sync-new-cart@test.invalid");
        var album = NewAlbum("Sync New Cart Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SyncCartCommand(
                user.PublicId,
                [new SyncCartItem(s_albumSqids.Encode(album.Id.Value), 2)]
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == album.Id && ci.Quantity == 2);
    }

    [Fact]
    public async Task Handle_ExistingCart_ReplacesItemsRatherThanMerging()
    {
        var user = await CreateUserAsync("sync-replace-cart@test.invalid");
        var previousAlbum = NewAlbum("Sync Replace Previous Album");
        var newAlbum = NewAlbum("Sync Replace New Album");
        DbContext.Albums.AddRange(previousAlbum, newAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbContext.Carts.Add(
            new Cart
            {
                UserId = user.Id,
                CartItems = [new CartItem { AlbumId = previousAlbum.Id, Quantity = 1 }],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SyncCartCommand(
                user.PublicId,
                [new SyncCartItem(s_albumSqids.Encode(newAlbum.Id.Value), 3)]
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == newAlbum.Id && ci.Quantity == 3);
    }

    [Fact]
    public async Task Handle_ExistingItemSyncedAgain_QuantityIsOverwrittenNotSummed()
    {
        var user = await CreateUserAsync("sync-overwrite-qty@test.invalid");
        var album = NewAlbum("Sync Overwrite Qty Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbContext.Carts.Add(
            new Cart
            {
                UserId = user.Id,
                CartItems = [new CartItem { AlbumId = album.Id, Quantity = 1 }],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SyncCartCommand(
                user.PublicId,
                [new SyncCartItem(s_albumSqids.Encode(album.Id.Value), 5)]
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == album.Id && ci.Quantity == 5);
    }

    [Fact]
    public async Task Handle_DuplicateSqidInRequest_QuantitiesAreSummed()
    {
        var user = await CreateUserAsync("sync-duplicate-sqid@test.invalid");
        var album = NewAlbum("Sync Duplicate Sqid Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sqid = s_albumSqids.Encode(album.Id.Value);
        var result = await Handler.Handle(
            new SyncCartCommand(
                user.PublicId,
                [new SyncCartItem(sqid, 2), new SyncCartItem(sqid, 3)]
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == album.Id && ci.Quantity == 5);
    }

    [Fact]
    public async Task Handle_MalformedOrUnknownSqid_IsSkippedWithoutError()
    {
        var user = await CreateUserAsync("sync-invalid-sqid@test.invalid");
        var album = NewAlbum("Sync Invalid Sqid Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SyncCartCommand(
                user.PublicId,
                [
                    new SyncCartItem("not-a-real-sqid", 1),
                    new SyncCartItem(s_albumSqids.Encode(999_999), 1),
                    new SyncCartItem(s_albumSqids.Encode(album.Id.Value), 1),
                ]
            ),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == album.Id);
    }

    [Fact]
    public async Task Handle_ConcurrentSyncsForSameNewItemOnExistingCart_BothSucceedWithoutDuplicateRow()
    {
        var user = await CreateUserAsync("sync-concurrent-new-item@test.invalid");
        var album = NewAlbum("Sync Concurrent New Item Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        DbContext.Carts.Add(new Cart { UserId = user.Id });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sqid = s_albumSqids.Encode(album.Id.Value);
        await using var dbContext2 = NewDbContext();
        var handler1 = Handler;
        var handler2 = new SyncCartHandler(dbContext2, s_albumSqids);

        var task1 = handler1.Handle(
            new SyncCartCommand(user.PublicId, [new SyncCartItem(sqid, 1)]),
            TestContext.Current.CancellationToken
        );
        var task2 = handler2.Handle(
            new SyncCartCommand(user.PublicId, [new SyncCartItem(sqid, 1)]),
            TestContext.Current.CancellationToken
        );
        var results = await Task.WhenAll(task1, task2);

        results.Should().OnlyContain(r => !r.IsError);
        var items = await DbContext
            .CartItems.Where(ci => ci.Cart.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Should().ContainSingle(ci => ci.AlbumId == album.Id && ci.Quantity == 1);
    }

    [Fact]
    public async Task Handle_ConcurrentFirstSyncsWithNoExistingCart_BothSucceedWithoutDuplicateCart()
    {
        var user = await CreateUserAsync("sync-concurrent-first-sync@test.invalid");
        var album = NewAlbum("Sync Concurrent First Sync Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sqid = s_albumSqids.Encode(album.Id.Value);
        await using var dbContext2 = NewDbContext();
        var handler1 = Handler;
        var handler2 = new SyncCartHandler(dbContext2, s_albumSqids);

        var task1 = handler1.Handle(
            new SyncCartCommand(user.PublicId, [new SyncCartItem(sqid, 1)]),
            TestContext.Current.CancellationToken
        );
        var task2 = handler2.Handle(
            new SyncCartCommand(user.PublicId, [new SyncCartItem(sqid, 1)]),
            TestContext.Current.CancellationToken
        );
        var results = await Task.WhenAll(task1, task2);

        results.Should().OnlyContain(r => !r.IsError);
        var cartCount = await DbContext.Carts.CountAsync(
            c => c.UserId == user.Id,
            TestContext.Current.CancellationToken
        );
        cartCount.Should().Be(1);
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

    private async Task<User> CreateUserAsync(string email)
    {
        await RegisterHandler.Handle(
            new RegisterCommand("Sync", "Test User", email, "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        return (await UserManager.FindByEmailAsync(email))!;
    }

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
