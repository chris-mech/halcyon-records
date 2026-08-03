using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.Domain;

public class Artist
{
    public ArtistId Id { get; set; }
    public required string Name { get; set; }
    public string? Bio { get; set; }
    public string? Origin { get; set; }
    public ArtistType? Type { get; set; }
    public int? SinceYear { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<AlbumArtist> AlbumArtists { get; set; } = [];
}
