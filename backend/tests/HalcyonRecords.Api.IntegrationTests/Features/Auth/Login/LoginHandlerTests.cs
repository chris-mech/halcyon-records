using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Features.Auth.Login;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.Login;

public class LoginHandlerTests(SqlServerContainerFixture fixture) : AuthIntegrationTestBase(fixture)
{
    private LoginHandler Handler => new(UserManager, JwtTokenService, DbContext, TimeProvider);

    private RegisterHandler RegisterHandler =>
        new(UserManager, JwtTokenService, DbContext, TimeProvider);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        await RegisterHandler.Handle(
            new RegisterCommand(
                "Valid",
                "Login User",
                "valid-login@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new LoginCommand("valid-login@test.invalid", "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorizedError()
    {
        var result = await Handler.Handle(
            new LoginCommand("unknown-email@test.invalid", "SomePassword123!"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorizedError()
    {
        await RegisterHandler.Handle(
            new RegisterCommand(
                "Wrong",
                "Password User",
                "wrong-password@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new LoginCommand("wrong-password@test.invalid", "IncorrectPassword123!"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_ValidCredentials_PersistsNewRefreshTokenHashedNotPlaintext()
    {
        await RegisterHandler.Handle(
            new RegisterCommand(
                "Hash",
                "Check Login User",
                "hash-check-login@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new LoginCommand("hash-check-login@test.invalid", "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        var user = await UserManager.FindByEmailAsync("hash-check-login@test.invalid");
        var storedToken = await DbContext.RefreshTokens.SingleAsync(
            rt =>
                rt.UserId == user!.Id
                && rt.TokenHash == JwtTokenService.HashToken(result.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        storedToken.TokenHash.Should().NotBe(result.Value.RefreshToken);
    }
}
