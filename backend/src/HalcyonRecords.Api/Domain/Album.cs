using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class Album
{
    public AlbumId Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Label { get; set; }
    public bool IsNew { get; set; }
    public bool IsStaffPick { get; set; }
    public string? ImageUrl { get; set; }
    public int UnitsInStock { get; set; }
    public int RestockUnitsInStock { get; set; }
    public int PriceInPence { get; set; }
    public int? OriginalPriceInPence { get; set; }

    public ICollection<AlbumArtist> AlbumArtists { get; set; } = [];
    public ICollection<AlbumGenre> AlbumGenres { get; set; } = [];
}
