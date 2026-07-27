using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class Artist
{
    public ArtistId Id { get; set; }
    public required string Name { get; set; }
    public string? Bio { get; set; }
    public string? Origin { get; set; }
    public int? ActiveSince { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<AlbumArtist> AlbumArtists { get; set; } = [];
}
