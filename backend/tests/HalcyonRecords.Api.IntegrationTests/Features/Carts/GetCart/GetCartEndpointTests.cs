using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Carts.GetCart;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Features.Carts.GetCart;

public class GetCartEndpointTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory = new(fixture);

    private JwtTokenService JwtTokenService { get; } =
        new(
            Options.Create(
                new JwtOptions
                {
                    SigningKey = ApiWebApplicationFactory.JwtSigningKey,
                    Issuer = ApiWebApplicationFactory.JwtIssuer,
                    Audience = ApiWebApplicationFactory.JwtAudience,
                }
            ),
            TimeProvider.System
        );

    public ValueTask InitializeAsync() => new(fixture.ResetAsync());

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task Get_NoAuthorizationHeader_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/cart", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_InvalidBearerToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "not-a-real-token"
        );

        var response = await client.GetAsync(new Uri("/api/cart", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ValidToken_EmptyCart_ReturnsEmptyArray()
    {
        var user = await CreateUserAsync("endpoint-empty-cart@test.invalid");
        var (accessToken, _) = JwtTokenService.GenerateAccessToken(user);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        var response = await client.GetAsync(new Uri("/api/cart", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<CartItemResponse>>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ValidToken_CartWithItems_ReturnsCartItems()
    {
        var user = await CreateUserAsync("endpoint-cart-with-items@test.invalid");
        var (accessToken, _) = JwtTokenService.GenerateAccessToken(user);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var album = new Album
            {
                Title = "Endpoint Cart Album",
                PriceInPence = 2000,
                UnitsInStock = 5,
            };
            dbContext.Albums.Add(album);
            await dbContext.SaveChangesAsync();

            dbContext.Carts.Add(
                new Cart
                {
                    UserId = user.Id,
                    CartItems = [new CartItem { AlbumId = album.Id, Quantity = 3 }],
                }
            );
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        var response = await client.GetAsync(new Uri("/api/cart", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<CartItemResponse>>();
        body.Should()
            .ContainSingle(item => item.Title == "Endpoint Cart Album" && item.Quantity == 3);
    }

    private async Task<User> CreateUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = "Endpoint",
            LastName = "User",
            RegisteredAt = TimeProvider.System.GetUtcNow(),
        };
        await userManager.CreateAsync(user, "ValidPassword123!");

        return user;
    }
}
