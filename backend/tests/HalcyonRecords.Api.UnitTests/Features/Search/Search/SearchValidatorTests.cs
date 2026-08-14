using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Search.Search;

namespace HalcyonRecords.Api.UnitTests.Features.Search.Search;

public class SearchValidatorTests
{
    private readonly SearchValidator _validator = new();

    [Fact]
    public void Q_IsValid_WhenNonEmpty()
    {
        var result = _validator.TestValidate(new SearchQuery("Search Title Match Album"));
        result.ShouldNotHaveValidationErrorFor(q => q.Q);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Q_IsInvalid_WhenEmptyOrWhitespace(string q)
    {
        var result = _validator.TestValidate(new SearchQuery(q));
        result.ShouldHaveValidationErrorFor(q => q.Q);
    }
}
