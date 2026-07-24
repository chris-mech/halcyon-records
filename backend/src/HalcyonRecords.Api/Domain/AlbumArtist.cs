using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class AlbumArtist
{
    public AlbumId AlbumId { get; set; }
    public ArtistId ArtistId { get; set; }
    public Album Album { get; set; } = default!;
    public Artist Artist { get; set; } = default!;
}
