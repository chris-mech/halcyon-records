using FluentAssertions;
using HalcyonRecords.Api.Common.Contracts;

namespace HalcyonRecords.Api.UnitTests.Common.Contracts;

public class PagedResultTests
{
    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(11, 5, 3)]
    [InlineData(1, 12, 1)]
    [InlineData(0, 12, 0)]
    public void TotalPages_RoundsUpToCoverAllItems(
        int totalCount,
        int pageSize,
        int expectedTotalPages
    )
    {
        var result = new PagedResult<int>([], Page: 1, PageSize: pageSize, TotalCount: totalCount);

        result.TotalPages.Should().Be(expectedTotalPages);
    }
}
