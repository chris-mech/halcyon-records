using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;

namespace HalcyonRecords.Api.UnitTests.Common.Sqids;

public class AlbumSqidEncoderTests
{
    private readonly AlbumSqidEncoder _encoder = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1234)]
    public void EncodeThenDecode_RoundTrips(int id)
    {
        var sqid = _encoder.Encode(id);

        _encoder.Decode(sqid).Should().Be(id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1234)]
    public void Decode_TruncatedSqid_ReturnsNull(int id)
    {
        var fullSqid = _encoder.Encode(id);
        var truncated = fullSqid[..^1];

        _encoder.Decode(truncated).Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-real-sqid!!")]
    public void Decode_InvalidInput_ReturnsNull(string sqid)
    {
        _encoder.Decode(sqid).Should().BeNull();
    }
}
