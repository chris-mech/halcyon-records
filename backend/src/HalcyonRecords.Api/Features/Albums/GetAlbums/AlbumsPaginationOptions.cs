namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class AlbumsPaginationOptions
{
    public const string SectionName = "AlbumsPagination";

    public int MinPage { get; init; } = 1;

    public int MinPageSize { get; init; } = 1;

    public int MaxPageSize { get; init; } = 50;

    public int DefaultPageSize { get; init; } = 12;
}
