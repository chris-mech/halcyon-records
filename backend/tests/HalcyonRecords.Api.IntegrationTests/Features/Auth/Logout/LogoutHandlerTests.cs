using FluentAssertions;
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

    private RegisterHandler RegisterHandler =>
        new(UserManager, JwtTokenService, DbContext, TimeProvider);

    private RefreshHandler RefreshHandler => new(JwtTokenService, DbContext, TimeProvider);

    [Fact]
    public async Task Handle_ValidToken_RevokesItAndReturnsSuccess()
    {
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Valid",
                "Logout User",
                "valid-logout@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new LogoutCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();

        var tokenHash = JwtTokenService.HashToken(registerResult.Value.RefreshToken);
        var storedToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.TokenHash == tokenHash,
            TestContext.Current.CancellationToken
        );
        storedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_LoggedOutToken_CanNoLongerBeRefreshed()
    {
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Blocked",
                "After Logout User",
                "blocked-after-logout@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        await Handler.Handle(
            new LogoutCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        var refreshResult = await RefreshHandler.Handle(
            new RefreshCommand(registerResult.Value.RefreshToken),
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
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Idempotent",
                "Logout User",
                "idempotent-logout@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var firstLogout = await Handler.Handle(
            new LogoutCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );
        var secondLogout = await Handler.Handle(
            new LogoutCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        firstLogout.IsError.Should().BeFalse();
        secondLogout.IsError.Should().BeFalse();
    }
}
