using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class AlbumGenre
{
    public AlbumId AlbumId { get; set; }
    public GenreId GenreId { get; set; }
    public Album Album { get; set; } = default!;
    public Genre Genre { get; set; } = default!;
}
