using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Auth.GetCurrentUser;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.GetCurrentUser;

public class GetCurrentUserEndpointTests(SqlServerContainerFixture fixture) : IAsyncLifetime
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

        var response = await client.GetAsync(new Uri("/api/auth/me", UriKind.Relative));

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

        var response = await client.GetAsync(new Uri("/api/auth/me", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ValidToken_ReturnsCurrentUser()
    {
        var user = await CreateUserAsync("endpoint-current-user@test.invalid", "Endpoint", "User");
        var (accessToken, _) = JwtTokenService.GenerateAccessToken(user);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        var response = await client.GetAsync(new Uri("/api/auth/me", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        body!.Email.Should().Be("endpoint-current-user@test.invalid");
        body.Id.Should().Be(user.PublicId);
    }

    private async Task<User> CreateUserAsync(string email, string firstName, string lastName)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            RegisteredAt = TimeProvider.System.GetUtcNow(),
        };
        await userManager.CreateAsync(user, "ValidPassword123!");

        return user;
    }
}
