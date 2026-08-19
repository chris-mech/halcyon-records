using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Features.Orders.GetOrders;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Orders.GetOrders;

public class GetOrdersHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();

    private GetOrdersHandler Handler => new(DbContext, s_albumSqids);

    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    [Fact]
    public async Task Handle_UnknownPublicId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            new GetOrdersQuery(Guid.NewGuid(), 1, 10),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_OrdersBelongingToOtherUsers_AreExcluded()
    {
        var owner = await CreateUserAsync("get-orders-owner@test.invalid");
        var otherUser = await CreateUserAsync("get-orders-other-user@test.invalid");
        var album = NewAlbum("Ownership Filter Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await AddOrderAsync(owner, album);
        await AddOrderAsync(otherUser, album);

        var result = await Handler.Handle(
            new GetOrdersQuery(owner.PublicId, 1, 10),
            TestContext.Current.CancellationToken
        );

        result.Value.Items.Should().ContainSingle();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MultipleOrders_ReturnsNewestFirst()
    {
        var user = await CreateUserAsync("get-orders-newest-first@test.invalid");
        var album = NewAlbum("Newest First Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var older = await AddOrderAsync(
            user,
            album,
            placedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var newer = await AddOrderAsync(
            user,
            album,
            placedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)
        );

        var result = await Handler.Handle(
            new GetOrdersQuery(user.PublicId, 1, 10),
            TestContext.Current.CancellationToken
        );

        result
            .Value.Items.Select(o => o.OrderNumber)
            .Should()
            .Equal(newer.OrderNumber, older.OrderNumber);
    }

    [Fact]
    public async Task Handle_PageBeyondTotalPages_ReturnsEmptyItemsNotError()
    {
        var user = await CreateUserAsync("get-orders-page-beyond@test.invalid");
        var album = NewAlbum("Page Beyond Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddOrderAsync(user, album);

        var result = await Handler.Handle(
            new GetOrdersQuery(user.PublicId, 5, 10),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OrderWithItems_MapsAlbumDetailsOntoEachItem()
    {
        var user = await CreateUserAsync("get-orders-item-mapping@test.invalid");
        var album = NewAlbum("Item Mapping Album", imageUrl: "https://example.test/cover.jpg");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await AddOrderAsync(user, album);

        var result = await Handler.Handle(
            new GetOrdersQuery(user.PublicId, 1, 10),
            TestContext.Current.CancellationToken
        );

        var order = result.Value.Items.Should().ContainSingle().Subject;
        order.Status.Should().Be(nameof(OrderStatus.Placed));
        var item = order.Items.Should().ContainSingle().Subject;
        item.AlbumSqid.Should().Be(s_albumSqids.Encode(album.Id.Value));
        item.Title.Should().Be("Item Mapping Album");
        item.TitleSlug.Should().Be(Slugifier.Slugify("Item Mapping Album"));
        item.ImageUrl.Should().Be("https://example.test/cover.jpg");
    }

    private async Task<Order> AddOrderAsync(
        User user,
        Album album,
        DateTimeOffset? placedAt = null,
        int quantity = 1
    )
    {
        var order = new Order
        {
            UserId = user.Id,
            OrderNumber = $"TEST-{Guid.NewGuid().ToString("N")[..10]}",
            IdempotencyKey = Guid.NewGuid(),
            TotalInPence = album.PriceInPence * quantity,
            PlacedAt = placedAt ?? DateTimeOffset.UtcNow,
            OrderItems =
            [
                new OrderItem
                {
                    AlbumId = album.Id,
                    Quantity = quantity,
                    PriceAtPurchaseInPence = album.PriceInPence,
                },
            ],
        };
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private async Task<User> CreateUserAsync(string email)
    {
        await RegisterHandler.Handle(
            new RegisterCommand("Get Orders", "Test User", email, "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        return (await UserManager.FindByEmailAsync(email))!;
    }

    private static Album NewAlbum(string title, int priceInPence = 1000, string? imageUrl = null) =>
        new()
        {
            Title = title,
            PriceInPence = priceInPence,
            ImageUrl = imageUrl,
        };
}
