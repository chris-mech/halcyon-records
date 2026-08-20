using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Orders.CreateOrder;

namespace HalcyonRecords.Api.UnitTests.Features.Orders.CreateOrder;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void ContactFirstName_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(contactFirstName: ""));
        result.ShouldHaveValidationErrorFor(c => c.ContactFirstName);
    }

    [Fact]
    public void ContactLastName_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(contactLastName: ""));
        result.ShouldHaveValidationErrorFor(c => c.ContactLastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void ContactEmail_IsInvalid_ForEmptyOrMalformedValues(string email)
    {
        var result = _validator.TestValidate(CreateCommand(contactEmail: email));
        result.ShouldHaveValidationErrorFor(c => c.ContactEmail);
    }

    [Fact]
    public void ContactEmail_IsValid_ForWellFormedAddress()
    {
        var result = _validator.TestValidate(
            CreateCommand(contactEmail: "well-formed@test.invalid")
        );
        result.ShouldNotHaveValidationErrorFor(c => c.ContactEmail);
    }

    [Fact]
    public void IdempotencyKey_IsInvalid_WhenEmpty()
    {
        var result = _validator.TestValidate(CreateCommand(idempotencyKey: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.IdempotencyKey);
    }

    private static CreateOrderCommand CreateCommand(
        string contactFirstName = "Valid",
        string contactLastName = "Contact",
        string contactEmail = "valid-command@test.invalid",
        Guid? idempotencyKey = null
    ) =>
        new(
            Guid.NewGuid(),
            contactFirstName,
            contactLastName,
            contactEmail,
            idempotencyKey ?? Guid.NewGuid()
        );
}
