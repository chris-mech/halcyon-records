namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed record OrderSummaryResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    string Status,
    int TotalInPence,
    IReadOnlyList<OrderSummaryItemResponse> Items
);

public sealed record OrderSummaryItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl
);
