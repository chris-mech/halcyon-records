namespace HalcyonRecords.Api.Features.Decades.GetDecades;

public sealed record DecadeListItemResponse(
    string Slug,
    string Label,
    int? StartYear,
    int? EndYear,
    string? ImageUrl,
    int AlbumCount
);
