using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.Register;

public class RegisterHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private RegisterHandler Handler => new(UserManager, JwtTokenService, DbContext, TimeProvider);

    [Fact]
    public async Task Handle_ValidRegistration_CreatesUserAndReturnsTokens()
    {
        var command = new RegisterCommand(
            "Valid",
            "Registration User",
            "valid-registration@test.invalid",
            "ValidPassword123!"
        );

        var result = await Handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var user = await UserManager.FindByEmailAsync("valid-registration@test.invalid");
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Valid");
        user.LastName.Should().Be("Registration User");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflictError()
    {
        const string email = "duplicate-email@test.invalid";
        await Handler.Handle(
            new RegisterCommand("First", "User", email, "ValidPassword123!"),
            TestContext.Current.CancellationToken
        );

        var result = await Handler.Handle(
            new RegisterCommand("Second", "User", email, "AnotherPassword123!"),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Auth.EmailAlreadyRegistered");
    }

    [Fact]
    public async Task Handle_WeakPassword_ReturnsValidationError()
    {
        var command = new RegisterCommand(
            "Weak",
            "Password User",
            "weak-password@test.invalid",
            "weak"
        );

        var result = await Handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.Errors.Should().OnlyContain(error => error.Type == ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ValidRegistration_PersistsRefreshTokenHashedNotPlaintext()
    {
        var command = new RegisterCommand(
            "Hash",
            "Check User",
            "hash-check@test.invalid",
            "ValidPassword123!"
        );

        var result = await Handler.Handle(command, TestContext.Current.CancellationToken);

        var user = await UserManager.FindByEmailAsync("hash-check@test.invalid");
        var storedToken = await DbContext.RefreshTokens.SingleAsync(
            rt => rt.UserId == user!.Id,
            TestContext.Current.CancellationToken
        );

        storedToken.TokenHash.Should().NotBe(result.Value.RefreshToken);
        storedToken.TokenHash.Should().Be(JwtTokenService.HashToken(result.Value.RefreshToken));
    }
}
