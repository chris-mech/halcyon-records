using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Decades.GetDecadeBySlug;

public class GetDecadeBySlugHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private GetDecadeBySlugHandler Handler => new(DbContext);

    [Fact]
    public async Task Handle_ExistingSlug_ReturnsDecadeWithAlbumCount()
    {
        DbContext.Decades.Add(
            new Decade
            {
                Slug = "1970s",
                Label = "1970s",
                StartYear = 1970,
                EndYear = 1979,
                Description = "A decade used to verify field mapping.",
            }
        );
        DbContext.Albums.Add(
            new Album
            {
                Title = "In Range Album",
                PriceInPence = 1000,
                ReleaseDate = new DateOnly(1975, 1, 1),
            }
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetDecadeBySlugQuery("1970s"),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.Label.Should().Be("1970s");
        result.Value.Description.Should().Be("A decade used to verify field mapping.");
        result.Value.AlbumCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_UnknownSlug_ReturnsNotFound()
    {
        var result = await Handler.Handle(
            new GetDecadeBySlugQuery("missing-decade"),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
