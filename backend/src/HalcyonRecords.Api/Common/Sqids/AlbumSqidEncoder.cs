using Sqids;

namespace HalcyonRecords.Api.Common.Sqids;

public sealed class AlbumSqidEncoder
{
    private readonly SqidsEncoder<int> _encoder = new(
        new SqidsOptions
        {
            Alphabet = "gvCi8aFhyjVq1ELk5tSwWURGOMp42ubnosl3z9IHZe6TcABQ7XdrDPNxfKYJ0m",
            MinLength = 6,
        }
    );

    public string Encode(int id) => _encoder.Encode(id);

    public int? Decode(string sqid) => _encoder.Decode(sqid) is [var id] ? id : null;
}