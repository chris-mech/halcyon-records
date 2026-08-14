namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed record DecadeDetailResponse(
    string Slug,
    string Label,
    int? StartYear,
    int? EndYear,
    string? Description,
    int AlbumCount
);
