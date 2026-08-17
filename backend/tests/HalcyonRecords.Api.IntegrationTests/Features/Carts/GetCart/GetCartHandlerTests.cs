using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Features.Carts.GetCart;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Carts.GetCart;

public class GetCartHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private GetCartHandler Handler => new(DbContext, s_albumSqids, s_artistSqids);

    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    [Fact]
    public async Task Handle_UnknownPublicId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            new GetCartQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_UserWithNoCart_ReturnsEmptyList()
    {
        var user = await CreateUserAsync("empty-cart@test.invalid");

        var result = await Handler.Handle(
            new GetCartQuery(user.PublicId),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CartWithItems_ReturnsAlbumDetailsAndArtists()
    {
        var user = await CreateUserAsync("cart-with-items@test.invalid");
        var artist = NewArtist("Cart Item Artist");
        var album = NewAlbum("Cart Item Album", priceInPence: 1500, unitsInStock: 4);
        Link(album, artist);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbContext.Carts.Add(
            new Cart
            {
                UserId = user.Id,
                CartItems = [new CartItem { AlbumId = album.Id, Quantity = 2 }],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetCartQuery(user.PublicId),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        var item = result.Value[0];
        item.AlbumSqid.Should().Be(s_albumSqids.Encode(album.Id.Value));
        item.Title.Should().Be("Cart Item Album");
        item.Quantity.Should().Be(2);
        item.PriceInPence.Should().Be(1500);
        item.UnitsInStock.Should().Be(4);
        item.IsInStock.Should().BeTrue();
        item.Artists.Should().ContainSingle(a => a.Name == "Cart Item Artist");
    }

    [Fact]
    public async Task Handle_AnotherUsersCart_IsNotReturned()
    {
        var owner = await CreateUserAsync("cart-owner@test.invalid");
        var otherUser = await CreateUserAsync("cart-other-user@test.invalid");
        var album = NewAlbum("Other User Album");
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        DbContext.Carts.Add(
            new Cart
            {
                UserId = owner.Id,
                CartItems = [new CartItem { AlbumId = album.Id, Quantity = 1 }],
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetCartQuery(otherUser.PublicId),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    private async Task<User> CreateUserAsync(string email)
    {
        await RegisterHandler.Handle(
            new RegisterCommand("Cart", "Test User", email, "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        return (await UserManager.FindByEmailAsync(email))!;
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Album NewAlbum(string title, int priceInPence = 1000, int unitsInStock = 10) =>
        new()
        {
            Title = title,
            PriceInPence = priceInPence,
            UnitsInStock = unitsInStock,
        };
}
