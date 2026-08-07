using Sqids;

namespace HalcyonRecords.Api.Common.Sqids;

public sealed class ArtistSqidEncoder
{
    private readonly SqidsEncoder<int> _encoder = new(
        new SqidsOptions
        {
            Alphabet = "WmLJeiKlZkqXo3CRUrYH0unc7Idtsv9NfQD4EV65BhxFz28wGTS1jMOaypbPgA",
            MinLength = 6,
        }
    );

    public string Encode(int id) => _encoder.Encode(id);

    public int? Decode(string sqid) => _encoder.Decode(sqid) is [var id] ? id : null;
}
