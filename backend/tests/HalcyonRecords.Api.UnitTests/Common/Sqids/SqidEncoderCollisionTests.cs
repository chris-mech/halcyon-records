using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;

namespace HalcyonRecords.Api.UnitTests.Common.Sqids;

public class SqidEncoderCollisionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public void AlbumAndArtistEncoders_ProduceDifferentSqidsForTheSameId(int id)
    {
        var albumSqid = new AlbumSqidEncoder().Encode(id);
        var artistSqid = new ArtistSqidEncoder().Encode(id);

        albumSqid.Should().NotBe(artistSqid);
    }
}
