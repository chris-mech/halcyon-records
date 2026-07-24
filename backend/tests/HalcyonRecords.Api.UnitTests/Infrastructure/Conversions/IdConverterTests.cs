using FluentAssertions;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure.Conversions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.UnitTests.Infrastructure.Conversions;

public class IdConverterTests
{
    [Fact]
    public void AlbumIdConverter_RoundTripsThroughProviderValue() =>
        AssertRoundTrips(new AlbumIdConverter(), new AlbumId(33));

    [Fact]
    public void ArtistIdConverter_RoundTripsThroughProviderValue() =>
        AssertRoundTrips(new ArtistIdConverter(), new ArtistId(33));

    [Fact]
    public void GenreIdConverter_RoundTripsThroughProviderValue() =>
        AssertRoundTrips(new GenreIdConverter(), new GenreId(33));

    private static void AssertRoundTrips<TId>(ValueConverter converter, TId id)
    {
        var providerValue = converter.ConvertToProvider(id);
        providerValue.Should().Be(33);

        var roundTripped = converter.ConvertFromProvider(providerValue);
        roundTripped.Should().Be(id);
    }
}
