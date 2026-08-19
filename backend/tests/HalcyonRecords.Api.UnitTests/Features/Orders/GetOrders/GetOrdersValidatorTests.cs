using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Orders.GetOrders;

namespace HalcyonRecords.Api.UnitTests.Features.Orders.GetOrders;

public class GetOrdersValidatorTests
{
    private readonly GetOrdersValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Page_IsValid_WhenGreaterThanZero(int page)
    {
        var result = _validator.TestValidate(CreateQuery(page: page));
        result.ShouldNotHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_IsInvalid_WhenNotGreaterThanZero(int page)
    {
        var result = _validator.TestValidate(CreateQuery(page: page));
        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    public void PageSize_IsValid_AtBoundaries(int pageSize)
    {
        var result = _validator.TestValidate(CreateQuery(pageSize: pageSize));
        result.ShouldNotHaveValidationErrorFor(q => q.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    public void PageSize_IsInvalid_OutsideBoundaries(int pageSize)
    {
        var result = _validator.TestValidate(CreateQuery(pageSize: pageSize));
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    private static GetOrdersQuery CreateQuery(int page = 1, int pageSize = 10) =>
        new(Guid.NewGuid(), page, pageSize);
}
