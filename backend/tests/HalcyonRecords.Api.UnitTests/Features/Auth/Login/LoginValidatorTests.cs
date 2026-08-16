using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Auth.Login;

namespace HalcyonRecords.Api.UnitTests.Features.Auth.Login;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void Email_IsInvalid_ForEmptyOrMalformedValues(string email)
    {
        var result = _validator.TestValidate(CreateCommand(email: email));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Email_IsValid_ForWellFormedAddress()
    {
        var result = _validator.TestValidate(CreateCommand(email: "well-formed@test.invalid"));
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Password_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(password: ""));
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Password_HasNoValidationError_ForShortValue()
    {
        var result = _validator.TestValidate(CreateCommand(password: "a"));
        result.ShouldNotHaveValidationErrorFor(c => c.Password);
    }

    private static LoginCommand CreateCommand(
        string email = "valid-command@test.invalid",
        string password = "ValidPassword123!"
    ) => new(email, password);
}
