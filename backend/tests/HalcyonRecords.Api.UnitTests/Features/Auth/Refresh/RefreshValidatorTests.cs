using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Auth.Refresh;

namespace HalcyonRecords.Api.UnitTests.Features.Auth.Refresh;

public class RefreshValidatorTests
{
    private readonly RefreshValidator _validator = new();

    [Fact]
    public void RefreshToken_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(new RefreshCommand(""));
        result.ShouldHaveValidationErrorFor(c => c.RefreshToken);
    }

    [Fact]
    public void RefreshToken_IsValid_WhenNonEmpty()
    {
        var result = _validator.TestValidate(new RefreshCommand("some-refresh-token"));
        result.ShouldNotHaveValidationErrorFor(c => c.RefreshToken);
    }
}
