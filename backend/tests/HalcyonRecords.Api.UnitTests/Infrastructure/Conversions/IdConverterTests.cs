using AutoFixture.Xunit3;
using FluentAssertions;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure.Conversions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.UnitTests.Infrastructure.Conversions;

public class IdConverterTests
{
    [Theory, AutoData]
    public void AlbumIdConverter_RoundTripsThroughProviderValue(AlbumId id) =>
        AssertRoundTrips(new AlbumIdConverter(), id, id.Value);

    [Theory, AutoData]
    public void ArtistIdConverter_RoundTripsThroughProviderValue(ArtistId id) =>
        AssertRoundTrips(new ArtistIdConverter(), id, id.Value);

    [Theory, AutoData]
    public void GenreIdConverter_RoundTripsThroughProviderValue(GenreId id) =>
        AssertRoundTrips(new GenreIdConverter(), id, id.Value);

    private static void AssertRoundTrips<TId>(
        ValueConverter converter,
        TId id,
        int expectedProviderValue
    )
    {
        var providerValue = converter.ConvertToProvider(id);
        providerValue.Should().Be(expectedProviderValue);

        var roundTripped = converter.ConvertFromProvider(providerValue);
        roundTripped.Should().Be(id);
    }
}
