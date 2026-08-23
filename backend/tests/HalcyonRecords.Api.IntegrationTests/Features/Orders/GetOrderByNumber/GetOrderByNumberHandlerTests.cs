using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Features.Orders.GetOrderByNumber;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Orders.GetOrderByNumber;

public class GetOrderByNumberHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private GetOrderByNumberHandler Handler => new(DbContext, s_albumSqids, s_artistSqids);
    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    [Fact]
    public async Task Handle_UnknownPublicId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            new GetOrderByNumberQuery(Guid.NewGuid(), "DOES-NOT-EXIST"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_UnknownOrderNumber_ReturnsNotFoundError()
    {
        var user = await CreateUserAsync("get-order-unknown-number@test.invalid");

        var result = await Handler.Handle(
            new GetOrderByNumberQuery(user.PublicId, "DOES-NOT-EXIST"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_OrderBelongsToAnotherUser_ReturnsNotFoundError_NotForbidden()
    {
        var owner = await CreateUserAsync("get-order-owner@test.invalid");
        var otherUser = await CreateUserAsync("get-order-other-user@test.invalid");
        var album = NewAlbum("Cross User Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var order = await AddOrderAsync(owner, album);

        var result = await Handler.Handle(
            new GetOrderByNumberQuery(otherUser.PublicId, order.OrderNumber),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_OwnOrder_ReturnsFullDetailIncludingItemPriceAndQuantity()
    {
        var user = await CreateUserAsync("get-order-own-detail@test.invalid");
        var album = NewAlbum(
            "Own Detail Album",
            priceInPence: 1500,
            imageUrl: "https://example.test/cover.jpg"
        );
        var artist = new Artist { Name = "Own Detail Artist" };
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var order = await AddOrderAsync(user, album, quantity: 2);

        var result = await Handler.Handle(
            new GetOrderByNumberQuery(user.PublicId, order.OrderNumber),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.OrderNumber.Should().Be(order.OrderNumber);
        result.Value.ContactFirstName.Should().Be("Order");
        result.Value.ContactLastName.Should().Be("Contact");
        result.Value.ContactEmail.Should().Be("order-contact@test.invalid");
        result.Value.Status.Should().Be(OrderStatus.Placed);
        result.Value.TotalInPence.Should().Be(3000);
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.AlbumSqid.Should().Be(s_albumSqids.Encode(album.Id.Value));
        item.TitleSlug.Should().Be(Slugifier.Slugify("Own Detail Album"));
        item.ImageUrl.Should().Be("https://example.test/cover.jpg");
        item.Quantity.Should().Be(2);
        item.PriceAtPurchaseInPence.Should().Be(1500);
        var itemArtist = item.Artists.Should().ContainSingle().Subject;
        itemArtist.Sqid.Should().Be(s_artistSqids.Encode(artist.Id.Value));
        itemArtist.Name.Should().Be("Own Detail Artist");
        itemArtist.NameSlug.Should().Be(Slugifier.Slugify("Own Detail Artist"));
    }

    private async Task<Order> AddOrderAsync(User user, Album album, int quantity = 1)
    {
        var order = new Order
        {
            UserId = user.Id,
            OrderNumber = $"TEST-{Guid.NewGuid().ToString("N")[..10]}",
            IdempotencyKey = Guid.NewGuid(),
            ContactFirstName = "Order",
            ContactLastName = "Contact",
            ContactEmail = "order-contact@test.invalid",
            TotalInPence = album.PriceInPence * quantity,
            PlacedAt = DateTimeOffset.UtcNow,
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
            new RegisterCommand("Get Order", "Test User", email, "ValidPassword123!"),
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
