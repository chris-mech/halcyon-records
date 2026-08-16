using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Features.Auth.Refresh;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.Refresh;

public class RefreshHandlerTests(SqlServerContainerFixture fixture) : AuthIntegrationTestBase(fixture)
{
    private RefreshHandler Handler => new(JwtTokenService, DbContext, TimeProvider);

    private RegisterHandler RegisterHandler =>
        new(UserManager, JwtTokenService, DbContext, TimeProvider);

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokensAndRotatesOldOne()
    {
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Valid",
                "Refresh User",
                "valid-refresh@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new RefreshCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.RefreshToken.Should().NotBe(registerResult.Value.RefreshToken);

        var oldTokenHash = JwtTokenService.HashToken(registerResult.Value.RefreshToken);
        var oldToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.TokenHash == oldTokenHash,
            TestContext.Current.CancellationToken
        );

        oldToken.RevokedAt.Should().NotBeNull();
        oldToken.ReplacedByTokenId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsUnauthorizedError()
    {
        var result = await Handler.Handle(
            new RefreshCommand("unknown-token-value"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("Auth.InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorizedError()
    {
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Expired",
                "Refresh User",
                "expired-refresh@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        TimeProvider.Advance(TimeSpan.FromDays(31));

        var result = await Handler.Handle(
            new RefreshCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Auth.InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ReusedToken_RevokesEntireDescendantFamily()
    {
        var registerResult = await RegisterHandler.Handle(
            new RegisterCommand(
                "Reuse",
                "Detection User",
                "reuse-detection@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var firstRefresh = await Handler.Handle(
            new RefreshCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );
        var secondRefresh = await Handler.Handle(
            new RefreshCommand(firstRefresh.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        var reuseResult = await Handler.Handle(
            new RefreshCommand(registerResult.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );

        reuseResult.IsError.Should().BeTrue();
        reuseResult.FirstError.Code.Should().Be("Auth.InvalidRefreshToken");

        var secondTokenHash = JwtTokenService.HashToken(firstRefresh.Value.RefreshToken);
        var secondToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.TokenHash == secondTokenHash,
            TestContext.Current.CancellationToken
        );
        secondToken.RevokedAt.Should().NotBeNull();

        var thirdTokenHash = JwtTokenService.HashToken(secondRefresh.Value.RefreshToken);
        var thirdToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.TokenHash == thirdTokenHash,
            TestContext.Current.CancellationToken
        );
        thirdToken.RevokedAt.Should().NotBeNull();

        var afterFamilyRevocation = await Handler.Handle(
            new RefreshCommand(secondRefresh.Value.RefreshToken),
            TestContext.Current.CancellationToken
        );
        afterFamilyRevocation.IsError.Should().BeTrue();
    }
}