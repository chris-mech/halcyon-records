using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Decades.GetDecades;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Decades.GetDecades;

public class GetDecadesHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private GetDecadesHandler Handler => new(DbContext);

    [Fact]
    public async Task Handle_ReturnsDecadesOrderedNewestFirst_WithOpenStartYearLast()
    {
        DbContext.Decades.AddRange(
            NewDecade("1970s", "1970s", 1970, 1979),
            NewDecade("2020s", "2020s", 2020, 2029),
            NewDecade("1960s-earlier", "1960s & earlier", null, 1969)
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetDecadesQuery(), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Select(d => d.Slug).Should().ContainInOrder("2020s", "1970s", "1960s-earlier");
    }

    [Fact]
    public async Task Handle_AlbumCount_OnlyCountsAlbumsWithinInclusiveYearRange()
    {
        DbContext.Decades.Add(NewDecade("1970s", "1970s", 1970, 1979));
        DbContext.Albums.AddRange(
            NewAlbum("Album Before Range", new DateOnly(1969, 1, 1)),
            NewAlbum("Album In Range", new DateOnly(1975, 1, 1)),
            NewAlbum("Album After Range", new DateOnly(1980, 1, 1)),
            NewAlbum("Album With No Release Date", null)
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetDecadesQuery(), CancellationToken.None);

        result.Value.Single(d => d.Slug == "1970s").AlbumCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AlbumCount_OpenStartYear_IncludesEverythingUpToEndYear()
    {
        DbContext.Decades.Add(NewDecade("1960s-earlier", "1960s & earlier", null, 1969));
        DbContext.Albums.AddRange(
            NewAlbum("Very Old Album", new DateOnly(1950, 1, 1)),
            NewAlbum("Just Inside Range", new DateOnly(1969, 1, 1)),
            NewAlbum("Just Outside Range", new DateOnly(1970, 1, 1))
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetDecadesQuery(), CancellationToken.None);

        result.Value.Single(d => d.Slug == "1960s-earlier").AlbumCount.Should().Be(2);
    }

    private static Decade NewDecade(string slug, string label, int? startYear, int? endYear) =>
        new()
        {
            Slug = slug,
            Label = label,
            StartYear = startYear,
            EndYear = endYear,
        };

    private static Album NewAlbum(string title, DateOnly? releaseDate) =>
        new()
        {
            Title = title,
            PriceInPence = 1000,
            ReleaseDate = releaseDate,
        };
}
