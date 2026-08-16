using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Auth.Logout;

namespace HalcyonRecords.Api.UnitTests.Features.Auth.Logout;

public class LogoutValidatorTests
{
    private readonly LogoutValidator _validator = new();

    [Fact]
    public void RefreshToken_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(new LogoutCommand(""));
        result.ShouldHaveValidationErrorFor(c => c.RefreshToken);
    }

    [Fact]
    public void RefreshToken_IsValid_WhenNonEmpty()
    {
        var result = _validator.TestValidate(new LogoutCommand("some-refresh-token"));
        result.ShouldNotHaveValidationErrorFor(c => c.RefreshToken);
    }
}
