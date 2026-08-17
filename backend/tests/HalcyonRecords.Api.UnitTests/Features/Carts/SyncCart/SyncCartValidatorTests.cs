using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Carts.SyncCart;

namespace HalcyonRecords.Api.UnitTests.Features.Carts.SyncCart;

public class SyncCartValidatorTests
{
    private readonly SyncCartValidator _validator = new();

    [Fact]
    public void Items_IsInvalid_WhenNull()
    {
        var result = _validator.TestValidate(new SyncCartCommand(Guid.NewGuid(), null!));
        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void Items_IsValid_WhenEmpty()
    {
        var result = _validator.TestValidate(new SyncCartCommand(Guid.NewGuid(), []));
        result.ShouldNotHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void ItemAlbumSqid_IsInvalid_WhenEmpty()
    {
        var command = new SyncCartCommand(Guid.NewGuid(), [new SyncCartItem("", 1)]);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].AlbumSqid");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ItemQuantity_IsInvalid_WhenNotPositive(int quantity)
    {
        var command = new SyncCartCommand(
            Guid.NewGuid(),
            [new SyncCartItem("some-sqid", quantity)]
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Item_IsValid_WhenWellFormed()
    {
        var command = new SyncCartCommand(Guid.NewGuid(), [new SyncCartItem("some-sqid", 1)]);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("Items[0].AlbumSqid");
        result.ShouldNotHaveValidationErrorFor("Items[0].Quantity");
    }
}
