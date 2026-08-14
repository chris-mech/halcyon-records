namespace HalcyonRecords.Api.Domain;

public class Decade
{
    public required string Slug { get; set; }
    public required string Label { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
