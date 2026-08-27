using FluentValidation.TestHelper;
using HalcyonRecords.Api.Features.Albums.GetAlbums;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.UnitTests.Features.Albums.GetAlbums;

public class GetAlbumsValidatorTests
{
    private readonly GetAlbumsValidator _validator = new(
        Options.Create(new AlbumsPaginationOptions())
    );

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
    [InlineData(50)]
    public void PageSize_IsValid_AtBoundaries(int pageSize)
    {
        var result = _validator.TestValidate(CreateQuery(pageSize: pageSize));
        result.ShouldNotHaveValidationErrorFor(q => q.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void PageSize_IsInvalid_OutsideBoundaries(int pageSize)
    {
        var result = _validator.TestValidate(CreateQuery(pageSize: pageSize));
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Genres_IsInvalid_WhenAnyEntryExceedsMaxLength()
    {
        var result = _validator.TestValidate(CreateQuery(genres: [new string('a', 151)]));
        result.ShouldHaveValidationErrorFor(q => q.Genres);
    }

    [Fact]
    public void Genres_IsInvalid_WhenMoreThanFiftySelected()
    {
        var genres = Enumerable.Range(0, 51).Select(i => $"genre-{i}").ToList();
        var result = _validator.TestValidate(CreateQuery(genres: genres));
        result.ShouldHaveValidationErrorFor(q => q.Genres);
    }

    [Fact]
    public void Genres_IsValid_AtExactlyFifty()
    {
        var genres = Enumerable.Range(0, 50).Select(i => $"genre-{i}").ToList();
        var result = _validator.TestValidate(CreateQuery(genres: genres));
        result.ShouldNotHaveValidationErrorFor(q => q.Genres);
    }

    [Fact]
    public void Genres_IsValid_WhenNull()
    {
        var result = _validator.TestValidate(CreateQuery(genres: null));
        result.ShouldNotHaveValidationErrorFor(q => q.Genres);
    }

    [Fact]
    public void EndYear_IsInvalid_WhenLessThanStartYear()
    {
        var result = _validator.TestValidate(CreateQuery(startYear: 1980, endYear: 1970));
        result.ShouldHaveValidationErrorFor(q => q.EndYear);
    }

    [Fact]
    public void EndYear_IsValid_WhenGreaterThanOrEqualToStartYear()
    {
        var result = _validator.TestValidate(CreateQuery(startYear: 1970, endYear: 1979));
        result.ShouldNotHaveValidationErrorFor(q => q.EndYear);
    }

    [Fact]
    public void EndYear_IsValid_WhenStartYearIsNull()
    {
        var result = _validator.TestValidate(CreateQuery(endYear: 1969));
        result.ShouldNotHaveValidationErrorFor(q => q.EndYear);
    }

    [Theory]
    [InlineData(nameof(AlbumSortBy.NewestFirst))]
    [InlineData(nameof(AlbumSortBy.OldestFirst))]
    [InlineData(nameof(AlbumSortBy.PriceAsc))]
    [InlineData(nameof(AlbumSortBy.PriceDesc))]
    [InlineData(nameof(AlbumSortBy.ArtistAZ))]
    [InlineData(nameof(AlbumSortBy.ArtistZA))]
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

    private static GetAlbumsQuery CreateQuery(
        int page = 1,
        int pageSize = 12,
        bool isNew = false,
        bool isOnSale = false,
        bool isStaffPick = false,
        bool inStock = false,
        IReadOnlyList<string>? genres = null,
        int? startYear = null,
        int? endYear = null,
        string sort = nameof(AlbumSortBy.NewestFirst)
    ) =>
        new(
            Page: page,
            PageSize: pageSize,
            IsNew: isNew,
            IsOnSale: isOnSale,
            IsStaffPick: isStaffPick,
            InStock: inStock,
            Genres: genres,
            StartYear: startYear,
            EndYear: endYear,
            Sort: sort
        );
}
