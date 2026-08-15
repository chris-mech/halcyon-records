using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Auth.Register;

namespace HalcyonRecords.Api.UnitTests.Features.Auth.Register;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void FirstName_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(firstName: ""));
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void LastName_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(lastName: ""));
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

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

    private static RegisterCommand CreateCommand(
        string firstName = "Valid",
        string lastName = "User",
        string email = "valid-command@test.invalid",
        string password = "ValidPassword123!"
    ) => new(firstName, lastName, email, password);
}
