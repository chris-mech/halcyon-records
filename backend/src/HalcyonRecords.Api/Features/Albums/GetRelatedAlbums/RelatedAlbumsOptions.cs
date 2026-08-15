namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed class RelatedAlbumsOptions
{
    public const string SectionName = "RelatedAlbums";

    public int MaxResults { get; init; } = 4;
}
