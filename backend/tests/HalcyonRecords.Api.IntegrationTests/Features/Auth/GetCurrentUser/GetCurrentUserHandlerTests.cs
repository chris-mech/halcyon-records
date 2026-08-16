using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Features.Auth.GetCurrentUser;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Auth.GetCurrentUser;

public class GetCurrentUserHandlerTests(SqlServerContainerFixture fixture)
    : AuthIntegrationTestBase(fixture)
{
    private GetCurrentUserHandler Handler => new(DbContext);

    private RegisterHandler RegisterHandler =>
        new(UserManager, JwtTokenService, DbContext, TimeProvider);

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDetails()
    {
        await RegisterHandler.Handle(
            new RegisterCommand(
                "Current",
                "User Test",
                "current-user@test.invalid",
                "ValidPassword123!"
            ),
            TestContext.Current.CancellationToken
        );

        var user = await UserManager.FindByEmailAsync("current-user@test.invalid");

        var result = await Handler.Handle(
            new GetCurrentUserQuery(user!.PublicId),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(user.PublicId);
        result.Value.Email.Should().Be("current-user@test.invalid");
        result.Value.FirstName.Should().Be("Current");
        result.Value.LastName.Should().Be("User Test");
    }

    [Fact]
    public async Task Handle_UnknownUserId_ReturnsNotFoundError()
    {
        var result = await Handler.Handle(
            new GetCurrentUserQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Auth.UserNotFound");
    }
}
