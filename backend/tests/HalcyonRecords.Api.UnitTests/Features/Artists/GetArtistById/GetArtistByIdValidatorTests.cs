using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Artists.GetArtistById;

namespace HalcyonRecords.Api.UnitTests.Features.Artists.GetArtistById;

public class GetArtistByIdValidatorTests
{
    private readonly GetArtistByIdValidator _validator = new();

    [Theory]
    [InlineData(nameof(ArtistAlbumSortBy.NewestFirst))]
    [InlineData(nameof(ArtistAlbumSortBy.OldestFirst))]
    [InlineData(nameof(ArtistAlbumSortBy.PriceAsc))]
    [InlineData(nameof(ArtistAlbumSortBy.PriceDesc))]
    public void Sort_IsValid_ForEveryRealEnumName(string sort)
    {
        var result = _validator.TestValidate(CreateQuery(sort: sort));
        result.ShouldNotHaveValidationErrorFor(q => q.Sort);
    }

    [Theory]
    [InlineData("notarealvalue")]
    [InlineData("newestfirst")]
    public void Sort_IsInvalid_ForUnrecognizedOrWrongCaseNames(string sort)
    {
        var result = _validator.TestValidate(CreateQuery(sort: sort));
        result.ShouldHaveValidationErrorFor(q => q.Sort);
    }

    private static GetArtistByIdQuery CreateQuery(
        string sqid = "abcdef",
        string sort = nameof(ArtistAlbumSortBy.NewestFirst)
    ) => new(sqid, sort);
}
