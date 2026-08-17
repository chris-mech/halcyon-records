using FluentAssertions;
using HalcyonRecords.Api.Features.Auth.Login;
using HalcyonRecords.Api.Features.Auth.Logout;
using HalcyonRecords.Api.Features.Auth.Refresh;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.Logout;

public class LogoutHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private LogoutHandler Handler => new(JwtTokenService, DbContext, TimeProvider);

    private RegisterHandler RegisterHandler => new(UserManager, TimeProvider);

    private LoginHandler LoginHandler => new(UserManager, JwtTokenService, DbContext, TimeProvider);

    private RefreshHandler RefreshHandler => new(JwtTokenService, DbContext, TimeProvider);

    private async Task<string> RegisterAndLoginAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        await RegisterHandler.Handle(
            new RegisterCommand(firstName, lastName, email, password),
            cancellationToken
        );

        var loginResult = await LoginHandler.Handle(
            new LoginCommand(email, password),
            cancellationToken
        );

        return loginResult.Value.RefreshToken;
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesItAndReturnsSuccess()
    {
        var refreshToken = await RegisterAndLoginAsync(
            "Valid",
            "Logout User",
            "valid-logout@test.invalid",
            "ValidPassword123!",
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new LogoutCommand(refreshToken),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();

        var tokenHash = JwtTokenService.HashToken(refreshToken);
        var storedToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.TokenHash == tokenHash,
            TestContext.Current.CancellationToken
        );
        storedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_LoggedOutToken_CanNoLongerBeRefreshed()
    {
        var refreshToken = await RegisterAndLoginAsync(
            "Blocked",
            "After Logout User",
            "blocked-after-logout@test.invalid",
            "ValidPassword123!",
            TestContext.Current.CancellationToken
        );

        await Handler.Handle(
            new LogoutCommand(refreshToken),
            TestContext.Current.CancellationToken
        );

        var refreshResult = await RefreshHandler.Handle(
            new RefreshCommand(refreshToken),
            TestContext.Current.CancellationToken
        );

        refreshResult.IsError.Should().BeTrue();
        refreshResult.FirstError.Code.Should().Be("Auth.InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsSuccessWithoutCreatingAnyRow()
    {
        var result = await Handler.Handle(
            new LogoutCommand("unknown-token-value"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AlreadyLoggedOutToken_IsIdempotent()
    {
        var refreshToken = await RegisterAndLoginAsync(
            "Idempotent",
            "Logout User",
            "idempotent-logout@test.invalid",
            "ValidPassword123!",
            TestContext.Current.CancellationToken
        );

        var firstLogout = await Handler.Handle(
            new LogoutCommand(refreshToken),
            TestContext.Current.CancellationToken
        );
        var secondLogout = await Handler.Handle(
            new LogoutCommand(refreshToken),
            TestContext.Current.CancellationToken
        );

        firstLogout.IsError.Should().BeFalse();
        secondLogout.IsError.Should().BeFalse();
    }
}
